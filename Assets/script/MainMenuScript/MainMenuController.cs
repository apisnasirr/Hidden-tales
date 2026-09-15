using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; 

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Swipe Navigation")]
    [SerializeField] private RectTransform _menuContainer; 
    [SerializeField] private float _slideDuration = 0.35f;
    [SerializeField] private float _swipeThreshold = 50f; 
    private float _screenWidth; // Calculated automatically now!

    [Header("Bottom Navigation Icons")]
    [Tooltip("The Image components on your 3 bottom buttons")]
    [SerializeField] private Image _keputusanIcon;
    [SerializeField] private Image _utamaIcon;
    [SerializeField] private Image _tetapanIcon;

    [Tooltip("The Blue (Active) versions of your icons")]
    [SerializeField] private Sprite _keputusanActive;
    [SerializeField] private Sprite _utamaActive;
    [SerializeField] private Sprite _tetapanActive;

    [Tooltip("The Gray (Inactive) versions of your icons")]
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
    [SerializeField] private string levelSelectSceneName = "LevelSelectScene"; 
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFX_VOLUME";

    private int _currentTabIndex = 1; 
    private Vector2 _startTouchPosition;
    private Vector2 _endTouchPosition;
    private bool _isSwiping = false;

    private void Start()
    {
        Time.timeScale = 1f;

        // Automatically find the exact width of your Canvas/Screen
        if (_menuContainer != null)
        {
            _screenWidth = _menuContainer.parent.GetComponent<RectTransform>().rect.width;
        }

        if (_creditsPopup != null) _creditsPopup.SetActive(false);
        if (_dimOverlay != null) _dimOverlay.SetActive(false);

        SetupSliders();
        LoadVolumeSettings();

        // Snap to Utama (Center) and update icons on start
        UpdateTabDisplay(0f); 
    }

    private void Update()
    {
        DetectSwipeInput();
    }

    // --- SWIPE LOGIC ---
    private void DetectSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _startTouchPosition = Input.mousePosition;
            _isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && _isSwiping)
        {
            _endTouchPosition = Input.mousePosition;
            _isSwiping = false;
            CalculateSwipe();
        }
    }

    private void CalculateSwipe()
    {
        float swipeDistance = _endTouchPosition.x - _startTouchPosition.x;
        float verticalDistance = _endTouchPosition.y - _startTouchPosition.y;

        if (Mathf.Abs(swipeDistance) > _swipeThreshold && Mathf.Abs(swipeDistance) > Mathf.Abs(verticalDistance))
        {
            if (swipeDistance > 0 && _currentTabIndex > 0)
            {
                _currentTabIndex--;
                UpdateTabDisplay(_slideDuration);
            }
            else if (swipeDistance < 0 && _currentTabIndex < 2)
            {
                _currentTabIndex++;
                UpdateTabDisplay(_slideDuration);
            }
        }
    }

    // --- BOTTOM NAVIGATION BUTTONS ---
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

        // Slide the panels
        float targetX = (_screenWidth * 1) - (_screenWidth * _currentTabIndex);
        _menuContainer.DOAnchorPosX(targetX, duration).SetEase(Ease.OutQuint);

        // Update the bottom navigation icons
        UpdateNavIcons();
    }

    private void UpdateNavIcons()
    {
        if (_keputusanIcon != null) _keputusanIcon.sprite = (_currentTabIndex == 0) ? _keputusanActive : _keputusanInactive;
        if (_utamaIcon != null) _utamaIcon.sprite = (_currentTabIndex == 1) ? _utamaActive : _utamaInactive;
        if (_tetapanIcon != null) _tetapanIcon.sprite = (_currentTabIndex == 2) ? _tetapanActive : _tetapanInactive;
    }

    // --- CREDITS POPUP ---
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

    // --- AUDIO SETTINGS ---
    private void SetupSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.AddListener(ApplyMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(ApplySFXVolume);
        }
    }

    private void LoadVolumeSettings()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        if (BGMManager.Instance != null) savedMusic = BGMManager.Instance.GetMusicVolume();
        if (SFXManager.Instance != null) savedSFX = SFXManager.Instance.GetSFXVolume();

        if (musicSlider != null) musicSlider.SetValueWithoutNotify(savedMusic);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(savedSFX);

        ApplyMusicVolume(savedMusic);
        ApplySFXVolume(savedSFX);
    }

    private void ApplyMusicVolume(float value)
    {
        if (BGMManager.Instance != null) BGMManager.Instance.SetMusicVolume(value);
        else PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    private void ApplySFXVolume(float value)
    {
        if (SFXManager.Instance != null) SFXManager.Instance.SetSFXVolume(value);
        else PlayerPrefs.SetFloat(SFXVolumeKey, value);
    }

    // --- SCENE LOADING ---
    public void LoadBandar() { PlayButtonSFX(); LoadSceneWithLoading(bandarSceneName); }
    public void LoadBengkel() { PlayButtonSFX(); LoadSceneWithLoading(bengkelSceneName); }
    public void LoadRuncit() { PlayButtonSFX(); LoadSceneWithLoading(runcitSceneName); }

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