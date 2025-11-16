using UnityEngine;
using TMPro;

public class Player2Controller : MonoBehaviour
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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal1");
        float vertical = Input.GetAxisRaw("Vertical1");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump1") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.M))
        {
            TakeDamage(20);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            GainTrust(20);
        }

        currentHealth = healthBar.GetHealth();
    }

    void SetBananaCount()
    {
        bananaCount.text = "P2 Bananas: " + count.ToString();
    }

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
