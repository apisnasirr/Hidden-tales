using System.Collections;
using TMPro;
using UnityEngine;

public class CoinPopupSpawner : MonoBehaviour
{
    public static CoinPopupSpawner Instance;

    [Header("Canvas")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Camera worldCamera;

    [Header("Existing Popup UI")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Destination Target")]
    [SerializeField] private RectTransform shopIconRect; // <-- NEW: Where the coin will fly to!

    [Header("Position")]
    [SerializeField] private Vector2 screenOffset = new Vector2(35f, 45f);

    [Header("Animation")]
    [SerializeField] private float hoverDuration = 0.4f; // Time spent floating up so the player can read it
    [SerializeField] private float flyDuration = 0.6f;   // Time spent flying to the shop
    [SerializeField] private float moveUpDistance = 60f;

    private RectTransform canvasRect;
    private Coroutine popupRoutine;
    private Vector3 defaultScale = Vector3.one;

    private void Awake()
    {
        Instance = this;

        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        if (targetCanvas != null)
            canvasRect = targetCanvas.GetComponent<RectTransform>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (popupRect == null && popupRoot != null)
            popupRect = popupRoot.GetComponent<RectTransform>();

        if (canvasGroup == null && popupRoot != null)
            canvasGroup = popupRoot.GetComponent<CanvasGroup>();

        if (canvasGroup == null && popupRoot != null)
            canvasGroup = popupRoot.AddComponent<CanvasGroup>();

        if (popupRect != null)
            defaultScale = popupRect.localScale;
    }

    public void ShowCoinPopup(Transform target, int amount)
    {
        if (target == null) return;
        ShowCoinPopup(target.position, amount);
    }

    public void ShowCoinPopup(Vector3 worldPosition, int amount)
    {
        if (targetCanvas == null || popupRoot == null || popupRect == null) return;

        if (canvasRect == null)
            canvasRect = targetCanvas.GetComponent<RectTransform>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        Vector2 screenPosition;
        if (worldCamera != null)
            screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        else
            screenPosition = worldPosition;

        screenPosition += screenOffset;
        Camera uiCamera = targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? targetCanvas.worldCamera : null;

        // 1. Figure out where the popup starts
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, uiCamera, out Vector2 localStartPosition
        );

        // 2. Figure out where the popup needs to fly to (The Shop Icon)
        Vector2 localEndPosition = localStartPosition + new Vector2(0f, moveUpDistance); // Fallback if shop icon is missing
        if (shopIconRect != null)
        {
            Vector2 shopScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, shopIconRect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, shopScreenPos, uiCamera, out localEndPosition
            );
        }

        // Reset the UI before animating
        popupRoot.SetActive(true);
        popupRect.SetAsLastSibling();
        popupRect.anchoredPosition = localStartPosition;
        popupRect.localScale = defaultScale;
        canvasGroup.alpha = 1f;

        if (amountText != null)
            amountText.text = "+" + amount;

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(AnimatePopup(localStartPosition, localEndPosition));
    }

    private IEnumerator AnimatePopup(Vector2 startPosition, Vector2 endPosition)
    {
        // PHASE 1: Hover up slightly so they can read the text
        float timer = 0f;
        Vector2 hoverPosition = startPosition + new Vector2(0f, moveUpDistance);

        while (timer < hoverDuration)
        {
            timer += Time.deltaTime;
            float t = timer / hoverDuration;
            
            // Ease out (starts fast, slows down)
            float easeOutT = 1f - (1f - t) * (1f - t);
            
            if (popupRect != null)
                popupRect.anchoredPosition = Vector2.Lerp(startPosition, hoverPosition, easeOutT);

            yield return null;
        }

        // PHASE 2: Zip to the Shop Icon and fade/shrink
        timer = 0f;
        Vector2 zipStartPosition = popupRect.anchoredPosition;

        while (timer < flyDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flyDuration;
            
            // Ease in (starts slow, zips fast)
            float easeInT = t * t;

            if (popupRect != null)
            {
                popupRect.anchoredPosition = Vector2.Lerp(zipStartPosition, endPosition, easeInT);
                
                // Only start shrinking in the second half of the flight
                float shrinkT = Mathf.Clamp01((t - 0.5f) * 2f); 
                popupRect.localScale = Vector3.Lerp(defaultScale, Vector3.zero, shrinkT);
            }

            if (canvasGroup != null)
            {
                // NEW: Wait until the last 20% of the flight before fading out!
                float fadeT = Mathf.Clamp01((t - 0.8f) / 0.2f);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        // Clean up
        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (popupRect != null)
            popupRect.localScale = defaultScale;

        popupRoutine = null;
    }
}