using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; 

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Swipe Navigation")]
    [Tooltip("The wide container holding all 3 panels side-by-side")]
    [SerializeField] private RectTransform _menuContainer; 
    [SerializeField] private float _slideDuration = 0.35f;
    private float _screenWidth; 

    [Header("Bottom Navigation Icons")]
    [SerializeField] private Image _keputusanIcon;
    [SerializeField] private Image _utamaIcon;
    [SerializeField] private Image _tetapanIcon;

    [SerializeField] private Sprite _keputusanActive;
    [SerializeField] private Sprite _utamaActive;
    [SerializeField] private Sprite _tetapanActive;

    [SerializeField] private Sprite _keputusanInactive;
    [SerializeField] private Sprite _utamaInactive;
    [SerializeField] private Sprite _tetapanInactive;

    [Header("Credits Popup")]
    [SerializeField] private GameObject _creditsPopup;
    [SerializeField] private GameObject _dimOverlay;

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Scene Names")]
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFX_VOLUME";

    private int _currentTabIndex = 1; 

    private void Start()
    {
        Time.timeScale = 1f;

        if (_menuContainer != null)
        {
            _screenWidth = _menuContainer.parent.GetComponent<RectTransform>().rect.width;
        }

        if (_creditsPopup != null) _creditsPopup.SetActive(false);
        if (_dimOverlay != null) _dimOverlay.SetActive(false);

        SetupSliders();
        LoadVolumeSettings();

        UpdateTabDisplay(0f); 
    }

    public void RequestSwipe(bool toRight)
    {
        if (toRight && _currentTabIndex < 2)
        {
            _currentTabIndex++;
            UpdateTabDisplay(_slideDuration);
        }
        else if (!toRight && _currentTabIndex > 0)
        {
            _currentTabIndex--;
            UpdateTabDisplay(_slideDuration);
        }
    }

    public void GoToKeputusan()
    {
        _currentTabIndex = 0;
        PlayButtonSFX();
        UpdateTabDisplay(_slideDuration);
    }

    public void GoToUtama()
    {
        _currentTabIndex = 1;
        PlayButtonSFX();
        UpdateTabDisplay(_slideDuration);
    }

    public void GoToTetapan()
    {
        _currentTabIndex = 2;
        PlayButtonSFX();
        UpdateTabDisplay(_slideDuration);
    }

    private void UpdateTabDisplay(float duration)
    {
        if (_menuContainer == null) return;

        float targetX = (_screenWidth * 1) - (_screenWidth * _currentTabIndex);
        _menuContainer.DOAnchorPosX(targetX, duration).SetEase(Ease.OutQuint);

        UpdateNavIcons();
    }

    private void UpdateNavIcons()
    {
        if (_keputusanIcon != null) _keputusanIcon.sprite = (_currentTabIndex == 0) ? _keputusanActive : _keputusanInactive;
        if (_utamaIcon != null) _utamaIcon.sprite = (_currentTabIndex == 1) ? _utamaActive : _utamaInactive;
        if (_tetapanIcon != null) _tetapanIcon.sprite = (_currentTabIndex == 2) ? _tetapanActive : _tetapanInactive;
    }

    public void OpenCredits()
    {
        PlayButtonSFX();
        if (_dimOverlay != null) _dimOverlay.SetActive(true);
        
        if (_creditsPopup != null)
        {
            _creditsPopup.SetActive(true);
            _creditsPopup.transform.localScale = Vector3.zero;
            _creditsPopup.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    public void CloseCredits()
    {
        PlayButtonSFX();
        
        if (_creditsPopup != null)
        {
            _creditsPopup.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                _creditsPopup.SetActive(false);
                if (_dimOverlay != null) _dimOverlay.SetActive(false);
            });
        }
    }

    private void SetupSliders() { /* ...Volume Setup... */ }
    private void LoadVolumeSettings() { /* ...Volume Load... */ }
    private void ApplyMusicVolume(float value) { /* ...Apply Music... */ }
    private void ApplySFXVolume(float value) { /* ...Apply SFX... */ }

    public void LoadBandar() { PlayButtonSFX(); LoadSceneWithLoading(bandarSceneName); }
    public void LoadBengkel() { PlayButtonSFX(); LoadSceneWithLoading(bengkelSceneName); }
    public void LoadRuncit() { PlayButtonSFX(); LoadSceneWithLoading(runcitSceneName); }

    private void LoadSceneWithLoading(string sceneName) { /* ...Scene Load... */ }
    private void PlayButtonSFX() { /* ...Button SFX... */ }
}