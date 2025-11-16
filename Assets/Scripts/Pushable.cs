using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pushable : MonoBehaviour
{
    public float requiredStrength = 10f;
    public float reKinematicDelay = 0.3f;

    Rigidbody rb;
    float defaultMaxDepenVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultMaxDepenVel = rb.maxDepenetrationVelocity;

        // Start kinematic so CC can't shove it
        rb.isKinematic = true;
        // Optional: zero de-penetration speed while kinematic (belt & suspenders)
        rb.maxDepenetrationVelocity = 0f;
    }

    public void TryPush(Vector3 worldDirection, float impulse, float pusherStrength)
    {
        if (pusherStrength < requiredStrength) return;

        // Allow physics to move it
        rb.isKinematic = false;
        rb.maxDepenetrationVelocity = defaultMaxDepenVel;

        // Apply your impulse
        worldDirection.y = 0f;
        worldDirection.Normalize();
        rb.AddForce(worldDirection * impulse, ForceMode.Impulse);

        // Re-lock after a short moment so idle contact can't nudge it
        CancelInvoke(nameof(ReKinematic));
        Invoke(nameof(ReKinematic), reKinematicDelay);
    }

    void ReKinematic()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.maxDepenetrationVelocity = 0f;
    }
}
