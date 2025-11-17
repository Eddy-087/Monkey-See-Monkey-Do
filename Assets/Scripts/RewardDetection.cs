using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class RewardDetection : MonoBehaviour
{
    public ThirdPersonUserControl player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Splash"))
        {
            other.gameObject.SetActive(false);
            player.GainTrust(5);
        }
    }
}
