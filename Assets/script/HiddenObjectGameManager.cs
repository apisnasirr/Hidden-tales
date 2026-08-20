using UnityEngine;
using UnityEngine.UI;

public class HiddenObjectGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalTargetObjects = 10;
    [SerializeField] private int maxLives = 3;

    [Header("UI")]
    [SerializeField] private Text collectedText;
    [SerializeField] private Text livesText;

    [Header("Managers")]
    [SerializeField] private LevelCompleteManager levelCompleteManager;
    [SerializeField] private GameOverManagerBandar gameOverManager;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip defeatSfx;
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip wrongSfx;

    private int collectedCount = 0;
    private int currentLives;
    private bool levelCompleted = false;
    private bool gameOver = false;

    private void Start()
    {
        currentLives = maxLives;
        UpdateUI();
    }

    public void HandleCorrectClick()
    {
        if (levelCompleted || gameOver)
            return;

        collectedCount++;

        PlaySfx(correctSfx);
        UpdateUI();

        if (collectedCount >= totalTargetObjects)
        {
            levelCompleted = true;

            PlaySfx(victorySfx);

            if (levelCompleteManager != null)
                levelCompleteManager.ShowCompletePanel();
        }
    }

    public void HandleWrongClick()
    {
        if (levelCompleted || gameOver)
            return;

        currentLives--;

        PlaySfx(wrongSfx);
        UpdateUI();

        if (currentLives <= 0)
        {
            currentLives = 0;
            gameOver = true;
            UpdateUI();

            PlaySfx(defeatSfx);

            if (gameOverManager != null)
                gameOverManager.ShowGameOverPanel();
        }
    }

    private void UpdateUI()
    {
        if (collectedText != null)
            collectedText.text = "Jumpa: " + collectedCount + "/" + totalTargetObjects;

        if (livesText != null)
            livesText.text = "Nyawa: " + currentLives;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
            sfxAudioSource.PlayOneShot(clip);
    }

    public int GetCollectedCount()
    {
        return collectedCount;
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }
}