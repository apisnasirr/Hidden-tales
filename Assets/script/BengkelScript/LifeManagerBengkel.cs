using UnityEngine;
using UnityEngine.UI;

public class LifeManagerBengkel : MonoBehaviour
{
    [Header("Life Settings")]
    [SerializeField] private int maxLives = 5;

    [Header("Heart UI")]
    [SerializeField] private Image[] hearts;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;

    [Header("Game Over")]
    [SerializeField] private GameOverManagerBengkel gameOverManager;

    private int currentLives;

    public int CurrentLives => currentLives;

    private void Start()
    {
        currentLives = maxLives;
        UpdateHeartsUI();
    }

    public void LoseLife()
    {
        if (currentLives <= 0) return;

        currentLives--;

        UpdateHeartsUI();

        Debug.Log("Nyawa tinggal: " + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private void UpdateHeartsUI()
    {
        int lostLives = maxLives - currentLives;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (i < lostLives)
                hearts[i].sprite = halfHeart;
            else
                hearts[i].sprite = fullHeart;
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");

        if (gameOverManager != null)
            gameOverManager.ShowGameOver();
    }
}