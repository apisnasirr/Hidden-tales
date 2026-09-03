using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject completePanel;

    [Header("Dependencies")]
    [SerializeField] private GameTimerManager timerManager; 

    [Header("Star Rating System")]
    [SerializeField] private float threeStarTimeLimit = 60f;  
    [SerializeField] private float twoStarTimeLimit = 150f;   
    [SerializeField] private Image star1Image;
    [SerializeField] private Image star2Image;
    [SerializeField] private Image star3Image;
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starEmptySprite;
    [SerializeField] private float starAnimDuration = 0.35f;
    [SerializeField] private float delayBetweenStars = 0.2f;

    [Header("Popup Animation Without Animator")]
    [SerializeField] private RectTransform popupTarget;
    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private float overshootScale = 1.08f;

    [Header("Scene Names")]
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string levelSelectSceneName = "LevelSelectScene"; // <-- NEW: Added this variable

    [Header("Optional")]
    [SerializeField] private bool pauseGameOnComplete = false;

    private bool hasCompleted = false;
    private Vector3 originalScale = Vector3.one;
    private Coroutine popupRoutine;

    private void Awake()
    {
        if (completePanel != null)
            completePanel.SetActive(false);

        if (popupTarget == null && completePanel != null)
            popupTarget = completePanel.GetComponent<RectTransform>();

        if (popupTarget != null)
            originalScale = popupTarget.localScale;
    }

    public void ShowLevelComplete()
    {
        if (hasCompleted)
            return;

        hasCompleted = true;

        if (timerManager != null)
            timerManager.StopTimer();

        if (star1Image != null) star1Image.transform.localScale = Vector3.zero;
        if (star2Image != null) star2Image.transform.localScale = Vector3.zero;
        if (star3Image != null) star3Image.transform.localScale = Vector3.zero;

        if (completePanel != null)
        {
            completePanel.SetActive(true);

            if (popupRoutine != null)
                StopCoroutine(popupRoutine);

            popupRoutine = StartCoroutine(PlayPopupAnimation());
        }

        if (BGMManager.Instance != null)
            BGMManager.Instance.PauseBGM();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayWin();

        if (pauseGameOnComplete)
            Time.timeScale = 0f;

        PlayerPrefs.SetInt("BengkelUnlocked", 1);
    }

    private IEnumerator PlayPopupAnimation()
    {
        if (popupTarget == null)
            yield break;

        popupTarget.localScale = Vector3.zero;

        float timer = 0f;
        Vector3 overshoot = originalScale * overshootScale;

        while (timer < popupDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / popupDuration);
            popupTarget.localScale = Vector3.Lerp(Vector3.zero, overshoot, EaseOutBack(t));
            yield return null;
        }

        float settleDuration = popupDuration * 0.5f;
        timer = 0f;

        while (timer < settleDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / settleDuration);
            popupTarget.localScale = Vector3.Lerp(overshoot, originalScale, t);
            yield return null;
        }

        popupTarget.localScale = originalScale;
        
        StartCoroutine(AnimateStarsRoutine());
        
        popupRoutine = null;
    }

    private IEnumerator AnimateStarsRoutine()
    {
        int starsEarned = 1; 

        if (timerManager != null)
        {
            float timeTaken = timerManager.GetTimeTaken();
            Debug.Log("[Star System] Time taken: " + timeTaken + " seconds.");

            if (timeTaken <= threeStarTimeLimit)
                starsEarned = 3;
            else if (timeTaken <= twoStarTimeLimit)
                starsEarned = 2;
        }

        Image[] stars = { star1Image, star2Image, star3Image };

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            stars[i].sprite = (i < starsEarned) ? starFilledSprite : starEmptySprite;
            
            float timer = 0f;
            while (timer < starAnimDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / starAnimDuration);
                stars[i].transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, EaseOutBack(t));
                yield return null;
            }

            stars[i].transform.localScale = Vector3.one;
            
            yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public void ShowCompletePanel() => ShowLevelComplete();
    public void GoToBengkel() { PlayButtonSFX(); LoadSceneWithLoading(bengkelSceneName); }
    public void GoToRuncit() { PlayButtonSFX(); LoadSceneWithLoading(runcitSceneName); }
    public void GoToMainMenu() { PlayButtonSFX(); LoadSceneWithLoading(mainMenuSceneName); }
    
    // --- NEW: Function to go to the Level Select Scene ---
    public void GoToLevelSelect() { PlayButtonSFX(); LoadSceneWithLoading(levelSelectSceneName); } 
    
    public void RestartLevel() { PlayButtonSFX(); LoadSceneWithLoading(SceneManager.GetActiveScene().name); }

    private void LoadSceneWithLoading(string sceneName)
    {
        Time.timeScale = 1f;
        if (LoadingScreenManager.Instance != null) LoadingScreenManager.Instance.LoadScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    private void PlayButtonSFX()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonClick();
    }
}