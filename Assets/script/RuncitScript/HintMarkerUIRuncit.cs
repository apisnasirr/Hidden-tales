using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HintMarkerUIRuncit : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 8f;        // How fast it bubbles
    [SerializeField] private float pulseIntensity = 0.2f;  // How much bigger/smaller it gets

    private RectTransform canvasRect;
    private Transform currentTarget;
    private Coroutine hideRoutine;
    private bool initialized;
    private Vector3 originalScale; // Remember the starting size

    private void Awake()
    {
        Initialize();
        HideInstant();
    }

    private void Initialize()
    {
        if (initialized) return;

        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        if (markerRect == null)
            markerRect = (RectTransform)transform;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (targetCanvas != null)
            canvasRect = targetCanvas.GetComponent<RectTransform>();

        // Capture the original size of the UI so the animation scales perfectly
        if (markerRect != null)
            originalScale = markerRect.localScale; 

        initialized = true;
    }

    private void LateUpdate()
    {
        if (canvasGroup == null || canvasGroup.alpha <= 0f || currentTarget == null)
            return;

        UpdateMarkerPosition();
    }

    public void ShowOnTarget(Transform target, float duration)
    {
        Initialize();

        currentTarget = target;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        ShowInstant();
        UpdateMarkerPosition();
        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float duration)
    {
        float time = 0f;

        // Loop runs until the timer is up
        while (time < duration)
        {
            time += Time.deltaTime;

            // --- Bubbly Animation Math ---
            if (markerRect != null)
            {
                float scaleModifier = 1f + (Mathf.Sin(time * pulseSpeed) * pulseIntensity);
                markerRect.localScale = originalScale * scaleModifier;
            }

            yield return null;
        }

        // Reset the scale back to normal before hiding it
        if (markerRect != null)
            markerRect.localScale = originalScale;

        HideInstant();
        hideRoutine = null;
    }

    private void ShowInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void HideInstant()
    {
        currentTarget = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void UpdateMarkerPosition()
    {
        if (worldCamera == null || targetCanvas == null || canvasRect == null || currentTarget == null)
            return;

        Vector2 screenPoint = worldCamera.WorldToScreenPoint(currentTarget.position + worldOffset);

        Camera uiCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : targetCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            uiCamera,
            out Vector2 localPoint
        );

        markerRect.anchoredPosition = localPoint;
    }
}