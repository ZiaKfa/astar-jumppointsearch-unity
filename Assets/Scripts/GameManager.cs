using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Player Stats")]
    [SerializeField] int maxHP = 5;
    [SerializeField] int currentHP;

    [Header("Score")]
    [SerializeField] int score;

    [Header("Events (Optional)")]
    public UnityEvent<int> OnHPChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnGameOver;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentHP = maxHP;
    }

    /* ==========================
       HP SYSTEM
       ========================== */

    public void DamagePlayer(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        OnHPChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            GameOver();
        }
    }

    public void HealPlayer(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        OnHPChanged?.Invoke(currentHP);
    }

    public int GetHP() => currentHP;

    public int GetMaxHP() => maxHP;
    /* ==========================
       SCORE SYSTEM
       ========================== */

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public int GetScore() => score;

    /* ==========================
       GAME STATE
       ========================== */

    void GameOver()
    {
        Debug.Log("GAME OVER");
        OnGameOver?.Invoke();
        Time.timeScale = 0f; // optional pause
    }
}
