using UnityEngine;

public class ObjectPush : MonoBehaviour
{
    public float pushPower;
    public float strength;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var pushable = hit.collider.GetComponent<Pushable>();
        if (pushable == null) return;

        // The direction the controller attempted to move this frame
        Vector3 pushDir = hit.moveDirection;
        pushDir.y = 0f;

        pushable.TryPush(pushDir, pushPower, strength);
    }

}
