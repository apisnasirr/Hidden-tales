using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManagerBandar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Scene Names")]
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFX_VOLUME";

    public bool IsSettingsOpen => settingsPanel != null && settingsPanel.activeSelf;
    public bool IgnoreNextWrongClick { get; private set; }

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

    public void OpenSettings()
    {
        IgnoreNextWrongClick = true;

        if (settingsPanel == null)
        {
            Debug.LogError("[SettingsManagerBandar] Settings Panel belum assign!");
            return;
        }

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();

        //if (BGMManager.Instance != null)
        //    BGMManager.Instance.PauseBGM();

        PlayButtonSFX();

        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        IgnoreNextWrongClick = true;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        //if (BGMManager.Instance != null)
        //    BGMManager.Instance.ResumeBGM();

        PlayButtonSFX();

        Time.timeScale = 1f;
    }

    public void ConsumeIgnoreNextWrongClick()
    {
        IgnoreNextWrongClick = false;
    }

    public void GoToBengkel()
    {
        IgnoreNextWrongClick = true;
        PlayButtonSFX();
        LoadSceneWithLoading(bengkelSceneName);
    }

    public void GoToRuncit()
    {
        IgnoreNextWrongClick = true;
        PlayButtonSFX();
        LoadSceneWithLoading(runcitSceneName);
    }

    public void GoToMainMenu()
    {
        IgnoreNextWrongClick = true;
        PlayButtonSFX();
        LoadSceneWithLoading(mainMenuSceneName);
    }

    public void RestartLevel()
    {
        IgnoreNextWrongClick = true;
        PlayButtonSFX();
        LoadSceneWithLoading(SceneManager.GetActiveScene().name);
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
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void ApplySFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (SFXManager.Instance != null)
            SFXManager.Instance.SetSFXVolume(value);
        else
        {
            PlayerPrefs.SetFloat(SFXVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void LoadSceneWithLoading(string sceneName)
    {
        Time.timeScale = 1f;

        Debug.Log("[SettingsManagerBandar] Nak load scene dengan loading: " + sceneName);

        if (LoadingScreenManager.Instance != null)
        {
            Debug.Log("[SettingsManagerBandar] LoadingScreenManager jumpa.");
            LoadingScreenManager.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("[SettingsManagerBandar] LoadingScreenManager NULL. Load direct.");
            SceneManager.LoadScene(sceneName);
        }
    }

    private void PlayButtonSFX()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }
}