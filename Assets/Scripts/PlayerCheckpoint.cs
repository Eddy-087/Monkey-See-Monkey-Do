using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour
{
    public string requiredTag = "Player";
    public Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
    //bool activated;

    void OnTriggerEnter(Collider other)
    {
        TrySet(other.gameObject);
    }

    void TrySet(GameObject go)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !go.CompareTag(requiredTag)) return;

        var pr = go.GetComponent<PlayerRespawn>();
        if (!pr) return;

        // Move/align a child "SpawnPoint" if you prefer exact facing/height.
        // Otherwise use our own transform + offset.
        Transform t = transform;
        Vector3 pos = t.position + spawnOffset;

        // Create a lightweight anchor at runtime so rotation/offset are preserved
        var anchor = new GameObject("CheckpointAnchor").transform;
        anchor.position = pos;
        anchor.rotation = Quaternion.identity; // or t.rotation if you want to preserve facing
        // Parent to this CP so it moves with moving platforms (optional):
        anchor.SetParent(transform);

        pr.SetCheckpoint(anchor);

        //activated = true;
    }
}
