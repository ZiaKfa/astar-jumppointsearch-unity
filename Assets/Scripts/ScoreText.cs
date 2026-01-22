using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    public Text scoreText;
    public GameController gameController;

    private void Update()
    {
        if (gameController != null)
        {
            scoreText.text = "Score: " + gameController.GetScore();
        }
    }
}