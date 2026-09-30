using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuIntro : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The panel containing your loading bar and background")]
    [SerializeField] private GameObject _loadingPanel;
    
    [Tooltip("The panel containing your MAIN and EXIT buttons")]
    [SerializeField] private GameObject _mainMenuPanel;

    [Header("Loading UI Elements")]
    [Tooltip("Drag your new LoadingFill image here")]
    [SerializeField] private Image _loadingFillImage; 
    [SerializeField] private TMP_Text _percentText;

    [Header("Settings")]
    [SerializeField] private float _introDuration = 3f;

    private void Start()
    {
        StartCoroutine(IntroLoadRoutine());
    }

    private IEnumerator IntroLoadRoutine()
    {
        if (_loadingPanel != null) _loadingPanel.SetActive(true);
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
        
        if (_loadingFillImage != null) _loadingFillImage.fillAmount = 0f;
        if (_percentText != null) _percentText.text = "0%";

        float timer = 0f;

        while (timer < _introDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / _introDuration);
            
            if (_loadingFillImage != null) _loadingFillImage.fillAmount = progress;
            if (_percentText != null) _percentText.text = Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }

        if (_loadingFillImage != null) _loadingFillImage.fillAmount = 1f;
        if (_percentText != null) _percentText.text = "100%";
        
        yield return new WaitForSeconds(0.5f); 

        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);
    }
}