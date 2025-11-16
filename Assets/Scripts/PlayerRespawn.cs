using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform checkpoint;
    Vector3 startPos;

    void Awake()
    {
        startPos = transform.position;
    }

    public void Respawn()
    {
        Vector3 target = checkpoint ? checkpoint.position : startPos;

        // Move instantly
        transform.position = target;
        transform.rotation = Quaternion.identity;
    }

    public void SetCheckpoint(Transform cp)
    {
        checkpoint = cp;
    }
}
