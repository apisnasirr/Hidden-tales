using UnityEngine;
using TMPro;

public class KeputusanManager : MonoBehaviour
{
    [Header("Total Stars")]
    [SerializeField] private TextMeshProUGUI _totalStarsText;
    private int _maxStars = 9;

    [Header("Bandar (Always Unlocked)")]
    [SerializeField] private GameObject[] _bandarStars; 
    [SerializeField] private TextMeshProUGUI _bandarTimeText;

    [Header("Bengkel")]
    [SerializeField] private GameObject _bengkelLockOverlay;
    [SerializeField] private GameObject[] _bengkelStars;
    [SerializeField] private TextMeshProUGUI _bengkelTimeText;

    [Header("Kedai Runcit")]
    [SerializeField] private GameObject _runcitLockOverlay;
    [SerializeField] private GameObject[] _runcitStars;
    [SerializeField] private TextMeshProUGUI _runcitTimeText;

    private void OnEnable()
    {
        RefreshKeputusanData();
    }

    public void RefreshKeputusanData()
    {
        int bandarStars = PlayerPrefs.GetInt("BandarStars", 0);
        int bengkelStars = PlayerPrefs.GetInt("BengkelStars", 0);
        int runcitStars = PlayerPrefs.GetInt("RuncitStars", 0);
        
        int totalStars = bandarStars + bengkelStars + runcitStars;
        if (_totalStarsText != null) _totalStarsText.text = $"{totalStars}/{_maxStars}";

        float bandarTime = PlayerPrefs.GetFloat("BandarBestTime", 0f);
        float bengkelTime = PlayerPrefs.GetFloat("BengkelBestTime", 0f);
        float runcitTime = PlayerPrefs.GetFloat("RuncitBestTime", 0f);

        bool isBengkelUnlocked = PlayerPrefs.GetInt("BengkelUnlocked", 0) == 1;
        bool isRuncitUnlocked = PlayerPrefs.GetInt("RuncitUnlocked", 0) == 1;

        UpdateLevelUI(true, bandarStars, bandarTime, _bandarStars, _bandarTimeText, null);
        UpdateLevelUI(isBengkelUnlocked, bengkelStars, bengkelTime, _bengkelStars, _bengkelTimeText, _bengkelLockOverlay);
        UpdateLevelUI(isRuncitUnlocked, runcitStars, runcitTime, _runcitStars, _runcitTimeText, _runcitLockOverlay);
    }

    private void UpdateLevelUI(bool isUnlocked, int starCount, float bestTime, GameObject[] starImages, TextMeshProUGUI timeText, GameObject lockOverlay)
    {
        if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].SetActive(isUnlocked && i < starCount);
            }
        }

        if (timeText != null)
        {
            if (!isUnlocked || bestTime <= 0f)
            {
                timeText.text = "????";
            }
            else
            {
                timeText.text = FormatTime(bestTime);
            }
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}