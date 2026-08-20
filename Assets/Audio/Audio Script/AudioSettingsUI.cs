using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sfxSlider;

    private bool isInitialized = false;

    private void Start()
    {
        if (sfxSlider == null)
            return;

        float savedVolume = 1f;

        if (SFXManager.Instance != null)
            savedVolume = SFXManager.Instance.GetSFXVolume();
        else
            savedVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);

        sfxSlider.SetValueWithoutNotify(savedVolume);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    public void OnSFXSliderChanged(float value)
    {
        if (!isInitialized)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.SetSFXVolume(value);

        Debug.Log("Slider ubah SFX volume ke: " + value);
    }
}