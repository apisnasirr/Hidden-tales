using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public void ResetGameProgress()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayButtonClick();
        }

        PlayerPrefs.DeleteAll();
        
        PlayerPrefs.Save();
        
        Debug.Log("[Settings] Game progress has been completely reset!");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ResetLevelsOnly()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonClick();

        PlayerPrefs.DeleteKey("BengkelUnlocked");
        PlayerPrefs.DeleteKey("RuncitUnlocked");

        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}