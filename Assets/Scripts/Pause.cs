using UnityEngine;

public class Pause : MonoBehaviour
{
    public void PauseGame()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Game Paused");
            Time.timeScale = 0f;
        }
    }

    public void Retry()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Retrying Level");
            Time.timeScale = 1f; // Ensure time scale is reset
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    public void ResumeGame()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Game Resumed");
            Time.timeScale = 1f;
        }
    }

    public void MainMenu()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Returning to Main Menu");
            Time.timeScale = 1f; // Ensure time scale is reset
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
}
