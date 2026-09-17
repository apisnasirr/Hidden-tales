using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using TMPro; // <-- NEW: Needed to update the Star Text!

public class LevelSelectController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu"; 
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";

    [Header("Bandar UI (Always Unlocked)")]
    [SerializeField] private GameObject bandarStarObj;
    [SerializeField] private TextMeshProUGUI bandarStarText;

    [Header("Bengkel UI & Locks")]
    [SerializeField] private Button bengkelButton;
    [SerializeField] private GameObject bengkelLockIcon;
    [SerializeField] private GameObject bengkelStarObj;
    [SerializeField] private TextMeshProUGUI bengkelStarText;
    
    [Header("Runcit UI & Locks")]
    [SerializeField] private Button runcitButton;
    [SerializeField] private GameObject runcitLockIcon;
    [SerializeField] private GameObject runcitStarObj;
    [SerializeField] private TextMeshProUGUI runcitStarText;

    private void Start()
    {
        CheckLevelState();
    }

    private void CheckLevelState()
    {
        bool isBengkelUnlocked = PlayerPrefs.GetInt("BengkelUnlocked", 0) == 1;
        bool isRuncitUnlocked = PlayerPrefs.GetInt("RuncitUnlocked", 0) == 1;

        int bandarStars = PlayerPrefs.GetInt("BandarStars", 0);
        int bengkelStars = PlayerPrefs.GetInt("BengkelStars", 0);
        int runcitStars = PlayerPrefs.GetInt("RuncitStars", 0);

        if (bandarStarObj != null) bandarStarObj.SetActive(true);
        if (bandarStarText != null) bandarStarText.text = $"{bandarStars}/3";

        if (bengkelButton != null) bengkelButton.interactable = isBengkelUnlocked;
        if (bengkelLockIcon != null) bengkelLockIcon.SetActive(!isBengkelUnlocked);
        
        if (bengkelStarObj != null) bengkelStarObj.SetActive(isBengkelUnlocked);
        if (bengkelStarText != null) bengkelStarText.text = $"{bengkelStars}/3";

        if (runcitButton != null) runcitButton.interactable = isRuncitUnlocked;
        if (runcitLockIcon != null) runcitLockIcon.SetActive(!isRuncitUnlocked);
        
        if (runcitStarObj != null) runcitStarObj.SetActive(isRuncitUnlocked);
        if (runcitStarText != null) runcitStarText.text = $"{runcitStars}/3";
    }

    public void BackToMainMenu()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(mainMenuSceneName);
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

    public void DebugResetData()
    {
        PlayerPrefs.SetInt("BengkelUnlocked", 0);
        PlayerPrefs.SetInt("RuncitUnlocked", 0);
        PlayerPrefs.SetInt("BandarStars", 0);
        PlayerPrefs.SetInt("BengkelStars", 0);
        PlayerPrefs.SetInt("RuncitStars", 0);
        PlayerPrefs.Save();
        CheckLevelState();
    }
}