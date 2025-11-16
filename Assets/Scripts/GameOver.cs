using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("MSMD Intro");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
