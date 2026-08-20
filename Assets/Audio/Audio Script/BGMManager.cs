using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private const string MusicVolumeKey = "MusicVolume";

    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        if (bgmClip != null)
            bgmSource.clip = bgmClip;

        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        bgmSource.volume = musicVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;

        RestartBGM();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestartBGM();
    }

    public void RestartBGM()
    {
        if (bgmSource == null)
            return;

        if (bgmSource.clip == null && bgmClip != null)
            bgmSource.clip = bgmClip;

        if (bgmSource.clip == null)
            return;

        bgmSource.Stop();
        bgmSource.time = 0f;
        bgmSource.volume = musicVolume;
        bgmSource.Play();
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource == null)
            return;

        if (bgmSource.clip == null && bgmClip != null)
            bgmSource.clip = bgmClip;

        if (bgmSource.clip != null && !bgmSource.isPlaying)
            bgmSource.UnPause();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();

        if (bgmSource != null)
            bgmSource.volume = musicVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }
}