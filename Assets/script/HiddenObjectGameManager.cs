using UnityEngine;

public class HiddenObjectGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalTargetObjects = 10;
    [SerializeField] private int maxLives = 3;

    [Header("Managers")]
    [SerializeField] private LevelCompleteManagerBengkel levelCompleteManager; 
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
    }

    public void HandleCorrectClick()
    {
        if (levelCompleted || gameOver) return;

        collectedCount++;
        PlaySfx(correctSfx);

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
        if (levelCompleted || gameOver) return;

        currentLives--;
        PlaySfx(wrongSfx);

        if (currentLives <= 0)
        {
            currentLives = 0;
            gameOver = true;
            PlaySfx(defeatSfx);

            if (gameOverManager != null)
                gameOverManager.ShowGameOverPanel();
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
            sfxAudioSource.PlayOneShot(clip);
    }

    public int GetCollectedCount() { return collectedCount; }
    public int GetCurrentLives() { return currentLives; }
    public bool IsLevelCompleted() { return levelCompleted; }
    public bool IsGameOver() { return gameOver; }
}