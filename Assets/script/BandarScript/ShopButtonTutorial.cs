using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopButtonTutorial : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The graphic that will pulse (The Shop Button itself)")]
    [SerializeField] private RectTransform shopButtonGraphic;
    
    [Tooltip("The static glow behind the button (Will NOT pulse)")]
    [SerializeField] private GameObject shopHighlightGraphic;

    [Header("Arrow Settings")]
    [Tooltip("Letak UI Image arrow dekat sini")]
    [SerializeField] private RectTransform arrowGraphic;
    [SerializeField] private float arrowBobSpeed = 6f;
    [SerializeField] private float arrowBobHeight = 15f; // Because UI is measured in pixels, we need a larger number like 10-20!

    [Header("Animation Settings")]
    [SerializeField] private float pulseScale = 1.15f; 
    [SerializeField] private float pulseSpeed = 6f;    

    private Vector3 originalScale;
    private Vector2 originalArrowPos;
    private Coroutine pulseCoroutine;
    private bool isPulsing = false;

    private void Awake()
    {
        if (shopButtonGraphic != null) 
            originalScale = shopButtonGraphic.localScale;
        
        if (shopHighlightGraphic != null) 
            shopHighlightGraphic.SetActive(false);

        // Hide arrow at start and remember its position
        if (arrowGraphic != null)
        {
            originalArrowPos = arrowGraphic.anchoredPosition;
            arrowGraphic.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        CharacterHighlight.OnTutorialCharacterCollected += StartShopPulse;
    }

    private void OnDisable()
    {
        CharacterHighlight.OnTutorialCharacterCollected -= StartShopPulse;
    }

    private void StartShopPulse()
    {
        if (gameObject.activeInHierarchy && !isPulsing)
        {
            isPulsing = true;
            if (pulseCoroutine != null) 
                StopCoroutine(pulseCoroutine);
                
            pulseCoroutine = StartCoroutine(PulseRoutine());
        }
    }

    private IEnumerator PulseRoutine()
    {
        if (shopHighlightGraphic != null) 
            shopHighlightGraphic.SetActive(true);

        if (arrowGraphic != null)
            arrowGraphic.gameObject.SetActive(true);

        float timer = 0f;

        while (isPulsing) 
        {
            timer += Time.unscaledDeltaTime; 
            
            // 1. Pulse the Button
            if (shopButtonGraphic != null)
            {
                float scaleAmount = 1f + (Mathf.Sin(timer * pulseSpeed) * (pulseScale - 1f));
                shopButtonGraphic.localScale = originalScale * scaleAmount;
            }

            // 2. Bob the Arrow
            if (arrowGraphic != null)
            {
                float newY = Mathf.Sin(timer * arrowBobSpeed) * arrowBobHeight;
                arrowGraphic.anchoredPosition = originalArrowPos + new Vector2(0f, newY);
            }
            
            yield return null;
        }
    }

    public void StopShopPulseAndHighlight()
    {
        isPulsing = false; 

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // Reset sizes and turn everything off
        if (shopButtonGraphic != null)
            shopButtonGraphic.localScale = originalScale;
            
        if (shopHighlightGraphic != null) 
            shopHighlightGraphic.SetActive(false);

        if (arrowGraphic != null)
        {
            arrowGraphic.anchoredPosition = originalArrowPos;
            arrowGraphic.gameObject.SetActive(false);
        }
    }
}