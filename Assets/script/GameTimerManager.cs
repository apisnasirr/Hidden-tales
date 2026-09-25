using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;

public class GameTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timerMinutes = 5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBarFill; 
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private float currentTime;
    private float totalTime; 
    private bool timerRunning = false;
    private bool hasGameOver = false;
    private bool isFrozen = false;
    private Coroutine freezeRoutine;
    private Color originalTextColor;

    private void Awake()
    {
        Time.timeScale = 1f;

        totalTime = timerMinutes * 60f; 
        currentTime = totalTime;
        timerRunning = true;
        hasGameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (timerText != null) 
            originalTextColor = timerText.color; 

        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerRunning || hasGameOver) return;

        if (!isFrozen)
        {
            currentTime -= Time.deltaTime;
        }

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
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        if (timerBarFill != null)
        {
            timerBarFill.fillAmount = currentTime / totalTime;
        }
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

    public float GetTimeTaken()
    {
        return totalTime - currentTime;
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

    public bool ApplyFreezeTime(float duration)
    {
        if (hasGameOver || !timerRunning || isFrozen) 
            return false; 

        if (freezeRoutine != null) 
            StopCoroutine(freezeRoutine);
            
        freezeRoutine = StartCoroutine(FreezeRoutine(duration));
        return true;
    }

    private System.Collections.IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;
        
        if (timerText != null) timerText.color = Color.cyan;

        yield return new WaitForSeconds(duration);

        isFrozen = false;
        
        if (timerText != null) timerText.color = originalTextColor; 
    }
}