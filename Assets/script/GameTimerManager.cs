using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timerMinutes = 5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private float currentTime;
    private bool timerRunning = false;
    private bool hasGameOver = false;

    private void Awake()
    {
        Time.timeScale = 1f;

        currentTime = timerMinutes * 60f;
        timerRunning = true;
        hasGameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerRunning || hasGameOver)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerUI();
            ShowGameOver();
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void ShowGameOver()
    {
        if (hasGameOver)
            return;

        hasGameOver = true;
        timerRunning = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (BGMManager.Instance != null)
            BGMManager.Instance.PauseBGM();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayDefeat();

        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(mainMenuSceneName);
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    private void LoadSceneWithLoading(string sceneName)
    {
        Time.timeScale = 1f;

        if (BGMManager.Instance != null)
            BGMManager.Instance.ResumeBGM();

        if (LoadingScreenManager.Instance != null)
            LoadingScreenManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    private void PlayButtonSFX()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }
}