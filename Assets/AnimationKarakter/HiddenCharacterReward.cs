using UnityEngine;

public class HiddenCharacterReward : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int coinReward = 5;

    [Header("Character SFX")]
    [SerializeField] private AudioClip characterClickSfx;

    [Header("Coin Popup")]
    [SerializeField] private bool showCoinPopup = true;
    [SerializeField] private CoinPopupSpawner coinPopupSpawner;

    [Header("Optional")]
    [SerializeField] private GameObject hiddenCharacterVisual;
    [SerializeField] private Animator animator;
    [SerializeField] private string revealTriggerName = "Reveal";

    private bool isCollected = false;
    private Collider col3D;
    private Collider2D col2D;
    private AudioSource audioSource;

    private void Awake()
    {
        if (hiddenCharacterVisual == null)
            hiddenCharacterVisual = gameObject;

        if (animator == null)
            animator = GetComponent<Animator>();

        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (coinPopupSpawner == null)
            coinPopupSpawner = FindObjectOfType<CoinPopupSpawner>();
    }

    private void OnMouseDown()
    {
        if (isCollected)
            return;

        isCollected = true;

        RegisterValidClickToWrongClickDetectors();

        PlaySfx();

        GiveCoinReward();

        PlayRevealAnimationOrShowVisual();

        DisableCollider();
    }

    private void RegisterValidClickToWrongClickDetectors()
    {
        WrongClickDetectorBandar detectorBandar = FindObjectOfType<WrongClickDetectorBandar>();
        if (detectorBandar != null)
            detectorBandar.RegisterValidClick();

        WrongClickDetectorBengkel detectorBengkel = FindObjectOfType<WrongClickDetectorBengkel>();
        if (detectorBengkel != null)
            detectorBengkel.RegisterValidClick();

        WrongClickDetectorRuncit detectorRuncit = FindObjectOfType<WrongClickDetectorRuncit>();
        if (detectorRuncit != null)
            detectorRuncit.RegisterValidClick();
    }

    private void PlaySfx()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayHiddenCharacter();
            SFXManager.Instance.PlayCoinGain();
        }

        if (characterClickSfx != null && audioSource != null)
            audioSource.PlayOneShot(characterClickSfx);
    }

    private void GiveCoinReward()
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyManager.Instance NULL masa klik hidden character");
            return;
        }

        CurrencyManager.Instance.AddCoins(coinReward);
        Debug.Log("Hidden character diklik -> coin masuk +" + coinReward);

        if (showCoinPopup)
            ShowCoinPopup();
    }

    private void ShowCoinPopup()
    {
        if (coinPopupSpawner == null)
            coinPopupSpawner = CoinPopupSpawner.Instance;

        if (coinPopupSpawner == null)
            coinPopupSpawner = FindObjectOfType<CoinPopupSpawner>();

        if (coinPopupSpawner == null)
        {
            Debug.LogWarning("[HiddenCharacterReward] CoinPopupSpawner tak jumpa dalam scene.");
            return;
        }

        coinPopupSpawner.ShowCoinPopup(transform, coinReward);
    }

    private void PlayRevealAnimationOrShowVisual()
    {
        if (animator != null && !string.IsNullOrEmpty(revealTriggerName))
        {
            animator.SetTrigger(revealTriggerName);
        }
        else if (hiddenCharacterVisual != null)
        {
            hiddenCharacterVisual.SetActive(true);
        }
    }

    private void DisableCollider()
    {
        if (col3D != null)
            col3D.enabled = false;

        if (col2D != null)
            col2D.enabled = false;
    }
}