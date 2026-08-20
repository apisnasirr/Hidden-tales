using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Scene Names")]
    [SerializeField] private string levelSelectSceneName = "LevelSelectScene"; // Your new scene!
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFX_VOLUME";

    private void Start()
    {
        Time.timeScale = 1f;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetupSliders();
        LoadVolumeSettings();
    }

    private void SetupSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.wholeNumbers = false;
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private void LoadVolumeSettings()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        // Updated to use the correct GetMusicVolume() from your BGMManager
        if (BGMManager.Instance != null)
            savedMusic = BGMManager.Instance.GetMusicVolume();

        if (SFXManager.Instance != null)
            savedSFX = SFXManager.Instance.GetSFXVolume();

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(savedMusic);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(savedSFX);

        ApplyMusicVolume(savedMusic);
        ApplySFXVolume(savedSFX);
    }

    public void OpenLevelSelect()
    {
        PlayButtonSFX();
        // Now it loads the new scene instead of opening a panel!
        LoadSceneWithLoading(levelSelectSceneName);
    }

    public void LoadBandar()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(bandarSceneName);
    }

    public void LoadBengkel()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(bengkelSceneName);
    }

    public void LoadRuncit()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(runcitSceneName);
    }

    public void OpenSettings()
    {
        PlayButtonSFX();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }
    }

    public void CloseSettings()
    {
        PlayButtonSFX();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (BGMManager.Instance != null)
            BGMManager.Instance.ResumeBGM();
    }

    public void QuitGame()
    {
        PlayButtonSFX();
        Application.Quit();
    }

    public void SetMusicVolume(float value)
    {
        ApplyMusicVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        ApplySFXVolume(value);
    }

    private void ApplyMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (BGMManager.Instance != null)
            BGMManager.Instance.SetMusicVolume(value);
        else
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    private void ApplySFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (SFXManager.Instance != null)
            SFXManager.Instance.SetSFXVolume(value);
        else
            PlayerPrefs.SetFloat(SFXVolumeKey, value);
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