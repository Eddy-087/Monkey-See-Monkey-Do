using UnityEngine;
using TMPro;

public class FinalScore : MonoBehaviour
{
    public int score_id;
    public TextMeshProUGUI score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (score_id == 1)
        {
            int playerScore = StaticData.p1_score;
            score.text = "Player 1 Score: " + playerScore.ToString();
        }
        else
        {
            int playerScore = StaticData.p2_score;
            score.text = "Player 2 Score: " + playerScore.ToString();
        }
    }
}
