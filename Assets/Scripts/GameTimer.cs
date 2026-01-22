using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    private float timeElapsed = 0f;
    private bool isRunning = false;
    public Text timerText;
    public GameController gameController;

    public void StartTimer()
    {
        isRunning = true;
        timeElapsed = 0f;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        timeElapsed = 0f;
        isRunning = false;
    }

    public float GetTimeElapsed()
    {
        return timeElapsed;
    }

    private void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        if (isRunning)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed % 0.5f < Time.deltaTime)
            {
                gameController.AddScore(1);
            }
            UpdateTimerUI();
        }
    }

    public void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60F);
        int seconds = Mathf.FloorToInt(timeElapsed - minutes * 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}