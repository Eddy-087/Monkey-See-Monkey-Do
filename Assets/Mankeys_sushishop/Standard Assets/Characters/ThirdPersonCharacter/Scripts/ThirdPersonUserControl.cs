using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(MonkeyMovement))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private MonkeyMovement m_Character; // A reference to the ThirdPersonCharacter on the object
        public Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

        public int player_id; 
        public HealthBar trustBar;
        private int maxTrust = 100;
        private int currentTrust = 50;

        private Coroutine healthDrainCoroutine;

        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<MonkeyMovement>();

            trustBar.SetMaxHealth(maxTrust);
            trustBar.SetHealth(currentTrust);
            healthDrainCoroutine = StartCoroutine(DrainHealthOverTime());
        }


        private void Update()
        {
            if (!m_Jump)
            {
                if (player_id == 1)
                {
                    m_Jump = Input.GetButtonDown("Jump");
                }
                else
                {
                    m_Jump = Input.GetButtonDown("Jump1");
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                TakeDamage(20);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GainTrust(20);
            }
            currentTrust = trustBar.GetHealth();

            if (currentTrust <= 0)
            {
                SceneManager.LoadScene("GameOverScreen");
                StopCoroutine(healthDrainCoroutine);
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h; //= CrossPlatformInputManager.GetAxis("Horizontal");
            float v; //= CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch; //= Input.GetKey(KeyCode.C);

            if (player_id == 1)
            {
                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");
                crouch = Input.GetKey(KeyCode.C);
                Debug.Log($"P{player_id} h:{h:F2} v:{v:F2} crouch:{crouch} jump:{m_Jump}");
            }
            else
            {
                h = Input.GetAxisRaw("Horizontal1");
                v = Input.GetAxisRaw("Vertical1");
                crouch = Input.GetKey(KeyCode.N);
                Debug.Log($"P{player_id} h:{h:F2} v:{v:F2} crouch:{crouch} jump:{m_Jump}");
            }

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
#if !MOBILE_INPUT
            // walk speed multiplier
            if (Input.GetKey(KeyCode.Z)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }

        public void TakeDamage(int damage)
        {
            currentTrust = trustBar.GetHealth() - damage;
            trustBar.SetHealth(currentTrust);
        }

        public void GainTrust(int trust)
        {
            currentTrust = trustBar.GetHealth() + trust;
            trustBar.SetHealth(currentTrust);
        }

        IEnumerator DrainHealthOverTime()
        {
            while (currentTrust > 0) // Continue draining as long as the player is alive
            {
                yield return new WaitForSeconds(5f); // Wait for 1 second
                TakeDamage(1); // Reduce health
            }
        }
    }
}