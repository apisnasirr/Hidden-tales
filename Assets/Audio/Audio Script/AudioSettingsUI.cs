using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider; // Make sure to assign this in the Inspector!

    private bool isInitialized = false;

    private void Start()
    {
        // 1. Initialize SFX Slider
        if (sfxSlider != null)
        {
            float savedSfxVolume = 1f;
            if (SFXManager.Instance != null)
                savedSfxVolume = SFXManager.Instance.GetSFXVolume();
            else
                savedSfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);

            sfxSlider.SetValueWithoutNotify(savedSfxVolume);
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }

        // 2. Initialize BGM Slider (Using your exact method names!)
        if (bgmSlider != null)
        {
            float savedBgmVolume = 1f;
            if (BGMManager.Instance != null) 
                savedBgmVolume = BGMManager.Instance.GetMusicVolume();
            else
                savedBgmVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

            bgmSlider.SetValueWithoutNotify(savedBgmVolume);
            bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMSliderChanged);
    }

    public void OnSFXSliderChanged(float value)
    {
        if (!isInitialized) return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.SetSFXVolume(value);
    }

    public void OnBGMSliderChanged(float value)
    {
        if (!isInitialized) return;

        if (BGMManager.Instance != null)
            BGMManager.Instance.SetMusicVolume(value);
    }
}