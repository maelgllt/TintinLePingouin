using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Level selection functions
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

    // Scene loading
    private void LoadGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}