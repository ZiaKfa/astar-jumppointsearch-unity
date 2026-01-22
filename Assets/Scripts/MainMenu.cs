using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void Benchmark()
    {
        // Load the benchmark scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Benchmark");
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }    
}

