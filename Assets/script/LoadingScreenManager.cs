using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("Loading UI (Main Wrapper)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text percentText;

    [Header("Level Info Panels")]
    [SerializeField] private GameObject bandarInfoPanel;
    [SerializeField] private GameObject bengkelInfoPanel;
    [SerializeField] private GameObject runcitInfoPanel;
    [SerializeField] private GameObject defaultInfoPanel; 

    [Header("Settings")]
    [SerializeField] private float loadingDuration = 3f;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        SetProgress(0f);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("[LoadingScreenManager] Safety lock was stuck on true! Forcing reset.");
            isLoading = false; 
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;
        Time.timeScale = 1f;

        Debug.Log("[LoadingScreenManager] Start loading: " + sceneName);

        if (BGMManager.Instance != null)
            BGMManager.Instance.PauseBGM();

        if (bandarInfoPanel != null) bandarInfoPanel.SetActive(false);
        if (bengkelInfoPanel != null) bengkelInfoPanel.SetActive(false);
        if (runcitInfoPanel != null) runcitInfoPanel.SetActive(false);
        if (defaultInfoPanel != null) defaultInfoPanel.SetActive(false);

        if (sceneName == "Bandar" && bandarInfoPanel != null)
            bandarInfoPanel.SetActive(true);
        else if (sceneName == "bengkel" && bengkelInfoPanel != null)
            bengkelInfoPanel.SetActive(true);
        else if (sceneName == "Kedai Runcit" && runcitInfoPanel != null)
            runcitInfoPanel.SetActive(true);
        else if (defaultInfoPanel != null)
            defaultInfoPanel.SetActive(true); 

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            loadingPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("[LoadingScreenManager] Loading Panel belum assign!");
        }

        SetProgress(0f);

        float timer = 0f;

        while (timer < loadingDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / loadingDuration);
            SetProgress(progress);

            yield return null;
        }

        SetProgress(1f);

        yield return new WaitForSecondsRealtime(0.3f);

        SceneManager.LoadScene(sceneName);

        yield return null;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        SetProgress(0f);
        isLoading = false;
    }

    private void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (loadingSlider != null)
            loadingSlider.value = progress;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}