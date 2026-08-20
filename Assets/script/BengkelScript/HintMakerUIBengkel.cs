using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HintMarkerUIBengkel : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;

    private RectTransform canvasRect;
    private Transform currentTarget;
    private Coroutine hideRoutine;
    private bool initialized;

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
        yield return new WaitForSeconds(duration);
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