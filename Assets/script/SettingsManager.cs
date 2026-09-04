using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public void ResetGameProgress()
    {
        // Optional: Play a button click sound if you have your SFXManager in the Main Menu
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayButtonClick();
        }

        // 1. This wipes EVERY PlayerPref you have saved (Level unlocks, coins, hints, stars)
        PlayerPrefs.DeleteAll();
        
        // 2. Force Unity to save this empty state immediately
        PlayerPrefs.Save();
        
        Debug.Log("[Settings] Game progress has been completely reset!");

        // 3. Reload the current scene (Main Menu) so the UI updates and levels lock again
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Use this INSTEAD if you only want to reset levels, but keep money/hints
    public void ResetLevelsOnly()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonClick();

        PlayerPrefs.DeleteKey("BengkelUnlocked");
        PlayerPrefs.DeleteKey("RuncitUnlocked");
        // Add any other specific keys here...

        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}