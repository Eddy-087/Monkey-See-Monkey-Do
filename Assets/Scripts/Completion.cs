using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.ThirdPerson;

public class Completion : MonoBehaviour
{
    private List<string> playerLayers = new List<string>();

    private void Update()
    {
        if (playerLayers.Count == 2)
        {
            SceneManager.LoadScene("Winner");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int playerLayer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(playerLayer);

        if (layerName == "Player1Layer")
        {
            if (!playerLayers.Contains(layerName))
            {
                playerLayers.Add(layerName);
                MonkeyMovement p1 = other.gameObject.GetComponent<MonkeyMovement>();
                StaticData.p1_score = p1.GetScore();
            }

            Debug.Log("Player 1 made it!!!");
        }
        if (layerName == "Player2Layer")
        {
            if (!playerLayers.Contains(layerName))
            {
                playerLayers.Add(layerName);
                MonkeyMovement p2 = other.gameObject.GetComponent<MonkeyMovement>();
                StaticData.p2_score = p2.GetScore();
            }
            Debug.Log("Player 2 made it");
        }
    }
}
