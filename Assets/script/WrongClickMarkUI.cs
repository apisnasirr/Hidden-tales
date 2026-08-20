using System.Collections;
using UnityEngine;

public class WrongClickMarkUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform markRect;
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private float showDuration = 0.4f;
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

    [Header("Ignore Click")]
    [SerializeField] private string hiddenCharacterTag = "HiddenCharacter";

    private Coroutine currentRoutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (markRect == null)
            markRect = GetComponent<RectTransform>();

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsClickOnHiddenCharacter(Vector2 screenPosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hit = Physics2D.OverlapPoint(point2D);

        if (hit != null && hit.CompareTag(hiddenCharacterTag))
        {
            return true;
        }

        return false;
    }

    public void ShowAtScreenPosition(Vector2 screenPosition)
    {
        if (markRect == null || parentCanvas == null)
            return;

        if (IsClickOnHiddenCharacter(screenPosition))
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        Vector2 finalScreenPos = screenPosition + offset;

        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        Camera uiCamera = null;
        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            finalScreenPos,
            uiCamera,
            out Vector2 localPoint))
        {
            markRect.anchoredPosition = localPoint;
        }

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(showDuration);
        canvasGroup.alpha = 0f;
        currentRoutine = null;
    }
}