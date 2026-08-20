using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteManagerBengkel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject completePanel;

    [Header("Popup Animation Without Animator")]
    [SerializeField] private RectTransform popupTarget;
    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private float overshootScale = 1.08f;

    [Header("Scene Names")]
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";
    [SerializeField] private string mainMenuSceneName = "Main Menu";

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

    public void ShowCompletePanel()
    {
        ShowLevelComplete();
    }

    public void ShowLevelComplete()
    {
        if (hasCompleted)
            return;

        hasCompleted = true;

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

            float t = timer / popupDuration;
            t = Mathf.Clamp01(t);

            popupTarget.localScale = Vector3.Lerp(Vector3.zero, overshoot, EaseOutBack(t));

            yield return null;
        }

        float settleDuration = popupDuration * 0.5f;
        timer = 0f;

        while (timer < settleDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / settleDuration;
            t = Mathf.Clamp01(t);

            popupTarget.localScale = Vector3.Lerp(overshoot, originalScale, t);

            yield return null;
        }

        popupTarget.localScale = originalScale;
        popupRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public void GoToBandar()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(bandarSceneName);
    }

    public void GoToRuncit()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(runcitSceneName);
    }

    public void GoToMainMenu()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(mainMenuSceneName);
    }

    public void RestartLevel()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(SceneManager.GetActiveScene().name);
    }

    private void LoadSceneWithLoading(string sceneName)
    {
        Time.timeScale = 1f;

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