using UnityEngine;
using System.Collections;

public class HiddenCharacterController : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int coinReward = 5;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string fullSequenceAnim = "RevealSequence";

    [Header("Movement")]
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveDuration = 1f;

    private bool isCollected = false;
    private bool isPlaying = false;
    private Collider col3D;
    private Collider2D col2D;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        if (isCollected || isPlaying)
            return;

        isCollected = true;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        isPlaying = true;

        WrongClickDetectorBandar bandar = FindObjectOfType<WrongClickDetectorBandar>();
        if (bandar != null)
            bandar.RegisterValidClick();

        WrongClickDetectorBengkel bengkel = FindObjectOfType<WrongClickDetectorBengkel>();
        if (bengkel != null)
            bengkel.RegisterValidClick();

        WrongClickDetectorRuncit runcit = FindObjectOfType<WrongClickDetectorRuncit>();
        if (runcit != null)
            runcit.RegisterValidClick();

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayHiddenCharacter();
            SFXManager.Instance.PlayCoinGain();
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCoins(coinReward);
            Debug.Log("Hidden character diklik. Coin +" + coinReward);
        }

        if (animator != null && !string.IsNullOrEmpty(fullSequenceAnim))
            animator.Play(fullSequenceAnim, 0, 0f);

        if (col3D != null) col3D.enabled = false;
        if (col2D != null) col2D.enabled = false;

        float timer = 0f;

        while (timer < moveDuration)
        {
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        isPlaying = false;
    }
}