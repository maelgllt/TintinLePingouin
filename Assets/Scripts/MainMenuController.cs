using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartEasyGame()
    {
        GameSettings.CurrentDifficulty = GameSettings.Difficulty.Easy;
        LoadGame();
    }

    public void StartMediumGame()
    {
        GameSettings.CurrentDifficulty = GameSettings.Difficulty.Medium;
        LoadGame();
    }

    public void StartHardGame()
    {
        GameSettings.CurrentDifficulty = GameSettings.Difficulty.Hard;
        LoadGame();
    }

    private void LoadGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void LeaveGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}