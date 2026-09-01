using System.Collections;
using UnityEngine;

public class ShopButtonTutorial : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform shopButtonRect;
    [Tooltip("Letak graphic bulat/glow kat belakang butang shop di sini")]
    [SerializeField] private GameObject shopHighlightGraphic;

    [Header("Animation Settings")]
    [SerializeField] private float pulseScale = 1.15f; // How big it gets
    [SerializeField] private float pulseSpeed = 6f;    // How fast it pulses
    [SerializeField] private float duration = 4f;      // How long the animation lasts before stopping

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        if (shopButtonRect == null) 
            shopButtonRect = GetComponent<RectTransform>();
            
        if (shopButtonRect != null) 
            originalScale = shopButtonRect.localScale;
        
        // Hide the highlight glow at start
        if (shopHighlightGraphic != null) 
            shopHighlightGraphic.SetActive(false);
    }

    private void OnEnable()
    {
        // Listen for the character to be collected
        CharacterHighlight.OnTutorialCharacterCollected += StartShopPulse;
    }

    private void OnDisable()
    {
        // Stop listening if the button is destroyed
        CharacterHighlight.OnTutorialCharacterCollected -= StartShopPulse;
    }

    private void StartShopPulse()
    {
        if (gameObject.activeInHierarchy)
        {
            if (pulseCoroutine != null) 
                StopCoroutine(pulseCoroutine);
                
            pulseCoroutine = StartCoroutine(PulseRoutine());
        }
    }

    private IEnumerator PulseRoutine()
    {
        // Turn on the glowing highlight behind the button
        if (shopHighlightGraphic != null) 
            shopHighlightGraphic.SetActive(true);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Unscaled so it works even if game is paused
            
            // Math to make it go big and small smoothly
            float scaleAmount = 1f + (Mathf.Sin(timer * pulseSpeed) * (pulseScale - 1f));
            shopButtonRect.localScale = originalScale * scaleAmount;
            
            yield return null;
        }

        // Reset everything back to normal when finished
        shopButtonRect.localScale = originalScale;
        if (shopHighlightGraphic != null) 
            shopHighlightGraphic.SetActive(false);
            
        pulseCoroutine = null;
    }
}