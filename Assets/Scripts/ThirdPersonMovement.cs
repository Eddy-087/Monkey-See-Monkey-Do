using UnityEngine;
using TMPro;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    public int maxHealth = 100;
    public int currentHealth = 50;
    public HealthBar healthBar;

    private int count;
    public TextMeshProUGUI bananaCount;

    public AnimatorController anim;

    void Start() 
    {
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
        count = 0;
        SetBananaCount();
    }

    // Update is called once per frame
    void Update()
    {
        // checks if the player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // when the player is on the ground after having fallen
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // represents its horizontal path
        float horizontal = Input.GetAxisRaw("Horizontal");
        // represents its vertical path
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        float movement = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
        anim.SetSpeed(movement);

        // complicated math that makes movement smoother, especially for movement at varying angles
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // when the player jumps and they're on the ground,
        // take gravity into account so that the player descends back to the ground
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            TakeDamage(20);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            GainTrust(20);
        }

        currentHealth = healthBar.GetHealth();
    }

    void SetBananaCount()
    {
        bananaCount.text = "P1 Bananas: " + count.ToString();
    }

    // for collecting bananas
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count += 1;
            SetBananaCount();
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth = healthBar.GetHealth() - damage;
        healthBar.SetHealth(currentHealth);
    }

    void GainTrust(int trust)
    {
        currentHealth = healthBar.GetHealth() + trust;
        healthBar.SetHealth(currentHealth);
    }
}
