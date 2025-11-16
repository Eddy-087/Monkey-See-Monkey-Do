using UnityEngine;

public class RestartZone : MonoBehaviour
{
    public string requiredTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        TryRespawn(other.gameObject);
    }

    void TryRespawn(GameObject go)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !go.CompareTag(requiredTag)) return;

        var respawn = go.GetComponent<PlayerRespawn>();
        if (respawn) respawn.Respawn();
    }
}
