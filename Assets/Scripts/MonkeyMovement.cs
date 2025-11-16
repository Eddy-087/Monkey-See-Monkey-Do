using UnityEngine;
using TMPro;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class MonkeyMovement : MonoBehaviour
    {
        [SerializeField] float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StationaryTurnSpeed = 180;
        [SerializeField] float m_JumpPower = 12f;
        [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] float m_RunCycleLegOffset = 0.2f;
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;

        [SerializeField] LayerMask m_HeadroomMask = Physics.AllLayers;
        [SerializeField] float m_HeadroomBuffer = 0.05f;

        CharacterController m_Controller;
        public ThirdPersonUserControl m_UserControl;
        Animator m_Animator;

        bool m_IsGrounded;
        float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;
        float m_TurnAmount;
        float m_ForwardAmount;
        Vector3 m_GroundNormal = Vector3.up;

        // Capsule dims from CharacterController
        float m_CapsuleHeight;
        Vector3 m_CapsuleCenter;
        bool m_Crouching;

        // We manage velocity ourselves with CC
        Vector3 m_Velocity;   // y used for gravity/jump; xz set from root motion or input

        public int player_id;
        private int count;
        public TextMeshProUGUI bananaCount;

        private bool hasKey = false;

        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Controller = GetComponent<CharacterController>();

            m_CapsuleHeight = m_Controller.height;
            m_CapsuleCenter = m_Controller.center;

            m_OrigGroundCheckDistance = m_GroundCheckDistance;

            // Ignore my own layer so CheckCapsule can't hit my CharacterController
            m_HeadroomMask &= ~(1 << gameObject.layer);

            count = 0;
            if (player_id == 1) bananaCount.text = "P1: " + count.ToString();
            else if (player_id == 2) bananaCount.text = "P2: " + count.ToString();

            m_Controller.Move(Vector3.down * 0.01f);
        }

        public void Move(Vector3 move, bool crouch, bool jump)
        {
            // Convert to local space and project on ground
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);

            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);

            m_TurnAmount = Mathf.Atan2(move.x, move.z);
            m_ForwardAmount = move.z;

            ApplyExtraTurnRotation();

            if (m_IsGrounded)
            {
                HandleGroundedMovement(crouch, jump);
            }
            else
            {
                HandleAirborneMovement();
            }

            ApplyCrouchState(crouch);

            UpdateAnimator(move);

            // Actually move the CharacterController here every frame
            Vector3 frameMove = new Vector3(m_Velocity.x, m_Velocity.y, m_Velocity.z) * Time.deltaTime;
            m_Controller.Move(frameMove);

            // After moving with CC, if grounded, zero small downward drift
            if (m_Controller.isGrounded && m_Velocity.y < 0f)
            {
                m_Velocity.y = -2f; // keep grounded
            }
        }

        bool HasHeadroom()
        {
            // Current top of the (possibly crouched) controller in world space
            Vector3 centerWorld = transform.TransformPoint(m_Controller.center);
            float currHalf = m_Controller.height * 0.5f;
            float standHalf = m_CapsuleHeight * 0.5f;

            // How much extra height we need to stand up
            float extra = Mathf.Max(0f, (standHalf - currHalf)) + m_HeadroomBuffer;

            if (extra <= 0.001f)
                return true; // already at full height

            // Start just below current top, cast upward only through the extra space
            Vector3 castStart = centerWorld + Vector3.up * (currHalf - (m_Controller.radius * 0.5f));
            float radius = Mathf.Max(0.01f, m_Controller.radius - 0.01f);

            // Ignore my own colliders via layer mask and root check
            const QueryTriggerInteraction qti = QueryTriggerInteraction.Ignore;
            RaycastHit hit;

            // Do the cast
            if (Physics.SphereCast(castStart, radius, Vector3.up, out hit, extra, m_HeadroomMask, qti))
            {
                // If it’s mine, ignore it and keep looking
                if (hit.collider && hit.collider.transform.root == transform.root)
                    return true; // treat as clear; our own colliders shouldn’t block

                return false; // something solid above: no headroom
            }

            return true;
        }

        void ApplyCrouchState(bool crouchInput)
        {
            bool wantsCrouch = (m_IsGrounded && crouchInput) || !HasHeadroom();

            if (wantsCrouch == m_Crouching) return; // no change

            if (wantsCrouch)
            {
                m_Controller.height = m_CapsuleHeight / 2f;
                // Move center so feet stay planted
                m_Controller.center = new Vector3(m_CapsuleCenter.x, m_CapsuleCenter.y / 2f, m_CapsuleCenter.z);
                m_Crouching = true;
            }
            else
            {
                m_Controller.height = m_CapsuleHeight;
                m_Controller.center = m_CapsuleCenter;
                m_Crouching = false;
            }
        }

        void UpdateAnimator(Vector3 move)
        {
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_Crouching);
            m_Animator.SetBool("OnGround", m_IsGrounded);
            if (!m_IsGrounded)
            {
                m_Animator.SetFloat("Jump", m_Velocity.y);
            }

            float runCycle = Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            m_Animator.speed = (m_IsGrounded && move.magnitude > 0) ? m_AnimSpeedMultiplier : 1f;
        }

        void HandleAirborneMovement()
        {
            // Manual gravity
            float gravity = Physics.gravity.y * m_GravityMultiplier;
            m_Velocity.y += gravity * Time.deltaTime;

            // While airborne, keep ground probe short when rising
            m_GroundCheckDistance = (m_Velocity.y < 0f) ? m_OrigGroundCheckDistance : 0.01f;
        }

        void HandleGroundedMovement(bool crouch, bool jump)
        {
            // Keep slight downward force to stick to ground
            if (m_Velocity.y < 0f) m_Velocity.y = -2f;

            if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                m_Velocity.y = m_JumpPower;
                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        }

        // Use animator root motion to set horizontal speed, then we pass it to CharacterController
        public void OnAnimatorMove()
        {
            if (Time.deltaTime <= 0f) return;

            // Prevent zeroing horizontal velocity while airborne
            if (!m_IsGrounded || !m_Animator.applyRootMotion) return;

            // Horizontal velocity from root motion (scaled), vertical from our own velocity
            Vector3 horizontal = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
            horizontal.y = 0f;

            // Convert world-space horizontal to our velocity buffer
            m_Velocity.x = horizontal.x;
            m_Velocity.z = horizontal.z;

            // If you prefer input-driven movement (ignoring root motion), comment the above and use:
            // var forward = transform.forward * m_ForwardAmount * m_MoveSpeedMultiplier;
            // m_Velocity.x = forward.x; m_Velocity.z = forward.z;
        }

        void CheckGroundStatus()
        {
            // Primary: use CC’s grounded flag
            m_IsGrounded = m_Controller.isGrounded;

#if UNITY_EDITOR
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f),
                           transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance),
                           Color.yellow);
#endif
            // Get a nicer ground normal via a short ray
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f),
                                 Vector3.down,
                                 out hitInfo,
                                 m_GroundCheckDistance + m_Controller.skinWidth + 0.2f))
            {
                m_GroundNormal = hitInfo.normal;
            }
            else
            {
                m_GroundNormal = Vector3.up;
            }

            // Let animator root motion apply only when grounded
            m_Animator.applyRootMotion = m_IsGrounded;
        }

        // NOTE: With CharacterController, this will only fire if the *other* collider has a Rigidbody.
        // Either add a kinematic Rigidbody to pickups, or keep a kinematic Rigidbody on the player.
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("PickUp"))
            {
                other.gameObject.SetActive(false);
                count += 1;
                if (player_id == 1) bananaCount.text = "P1: " + count.ToString();
                else if (player_id == 2) bananaCount.text = "P2: " + count.ToString();
                m_UserControl.GainTrust(1);
            }
            if (other.gameObject.CompareTag("Key")) 
            {
                other.gameObject.SetActive(false);
                hasKey = true;
            }
            if (other.gameObject.CompareTag("Door"))
            {
                if (hasKey)
                {
                    other.gameObject.SetActive(false);
                    m_UserControl.GainTrust(10);
                    hasKey = false;
                }
            }
            if (other.gameObject.CompareTag("Checkpoint1"))
            {
                other.gameObject.SetActive(false);
                m_UserControl.GainTrust(10);
            }
        }
    }
}

