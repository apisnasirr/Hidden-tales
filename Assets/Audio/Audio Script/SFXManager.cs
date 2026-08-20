using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    private const string SfxVolumeKey = "SFX_VOLUME";

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("General SFX")]
    [SerializeField] private AudioClip buttonClickSfx;
    [SerializeField] private AudioClip correctClickSfx;
    [SerializeField] private AudioClip wrongClickSfx;
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip defeatSfx;
    [SerializeField] private AudioClip coinGainSfx;
    [SerializeField] private AudioClip coinUseSfx;
    [SerializeField] private AudioClip notEnoughCoinSfx;
    [SerializeField] private AudioClip hiddenCharacterSfx;

    [Header("Bandar Animals")]
    [SerializeField] private AudioClip catSfx;
    [SerializeField] private AudioClip birdSfx;
    [SerializeField] private AudioClip frogSfx;

    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;

        LoadVolume();
        ApplyVolume();
    }

    private void LoadVolume()
    {
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    }

    private void ApplyVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();

        ApplyVolume();

        Debug.Log("SFX Volume set to: " + sfxVolume);
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    private void Play(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SFXManager] AudioClip belum assign.");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogWarning("[SFXManager] AudioSource tak jumpa.");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick() => Play(buttonClickSfx);
    public void PlayCorrectClick() => Play(correctClickSfx);
    public void PlayWrongClick() => Play(wrongClickSfx);
    public void PlayWin() => Play(victorySfx);
    public void PlayDefeat() => Play(defeatSfx);
    public void PlayCoinGain() => Play(coinGainSfx);
    public void PlayCoinUse() => Play(coinUseSfx);
    public void PlayNotEnoughCoin() => Play(notEnoughCoinSfx);
    public void PlayHiddenCharacter() => Play(hiddenCharacterSfx);

    public void PlayCat() => Play(catSfx);
    public void PlayBird() => Play(birdSfx);
    public void PlayFrog() => Play(frogSfx);
}