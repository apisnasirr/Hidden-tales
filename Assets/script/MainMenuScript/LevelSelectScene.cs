using UnityEngine;
using UnityEngine.UI; // <-- NEW: Needed for Button components
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu"; 
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";

    [Header("Level Buttons & Locks")]
    [SerializeField] private Button bengkelButton;
    [SerializeField] private GameObject bengkelLockIcon;
    
    [SerializeField] private Button runcitButton;
    [SerializeField] private GameObject runcitLockIcon;

    private void Start()
    {
        // Check the locks as soon as the menu opens!
        CheckLevelLocks();
    }

    private void CheckLevelLocks()
    {
        // Bandar is always unlocked, so we only check Bengkel and Runcit.
        // PlayerPrefs will check if it equals 1 (Unlocked). If it doesn't exist yet, it defaults to 0 (Locked).
        bool isBengkelUnlocked = PlayerPrefs.GetInt("BengkelUnlocked", 0) == 1;
        bool isRuncitUnlocked = PlayerPrefs.GetInt("RuncitUnlocked", 0) == 1;

        // Toggle Bengkel Button
        if (bengkelButton != null) bengkelButton.interactable = isBengkelUnlocked;
        if (bengkelLockIcon != null) bengkelLockIcon.SetActive(!isBengkelUnlocked);

        // Toggle Runcit Button
        if (runcitButton != null) runcitButton.interactable = isRuncitUnlocked;
        if (runcitLockIcon != null) runcitLockIcon.SetActive(!isRuncitUnlocked);
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

    // Optional: Call this from a debug button if you ever need to relock the levels for testing!
    public void DebugResetLocks()
    {
        PlayerPrefs.SetInt("BengkelUnlocked", 0);
        PlayerPrefs.SetInt("RuncitUnlocked", 0);
        PlayerPrefs.Save();
        CheckLevelLocks();
    }
}