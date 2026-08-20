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

    [Header("Position")]
    [SerializeField] private Vector2 screenOffset = new Vector2(35f, 45f);

    [Header("Animation")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float moveUpDistance = 60f;

    private RectTransform canvasRect;
    private Coroutine popupRoutine;

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
    }

    public void ShowCoinPopup(Transform target, int amount)
    {
        if (target == null)
        {
            Debug.LogWarning("[CoinPopupSpawner] Target character kosong.");
            return;
        }

        ShowCoinPopup(target.position, amount);
    }

    public void ShowCoinPopup(Vector3 worldPosition, int amount)
    {
        if (targetCanvas == null)
        {
            Debug.LogWarning("[CoinPopupSpawner] Target Canvas belum assign.");
            return;
        }

        if (canvasRect == null)
            canvasRect = targetCanvas.GetComponent<RectTransform>();

        if (popupRoot == null)
        {
            Debug.LogWarning("[CoinPopupSpawner] Popup Root belum assign.");
            return;
        }

        if (popupRect == null)
            popupRect = popupRoot.GetComponent<RectTransform>();

        if (popupRect == null)
        {
            Debug.LogWarning("[CoinPopupSpawner] Popup Root tiada RectTransform.");
            return;
        }

        if (canvasGroup == null)
            canvasGroup = popupRoot.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = popupRoot.AddComponent<CanvasGroup>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        Vector2 screenPosition;

        if (worldCamera != null)
        {
            screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        }
        else
        {
            screenPosition = worldPosition;
        }

        screenPosition += screenOffset;

        Camera uiCamera = null;

        if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = targetCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition
        );

        popupRoot.SetActive(true);
        popupRect.SetAsLastSibling();
        popupRect.anchoredPosition = localPosition;

        if (amountText != null)
            amountText.text = "+" + amount;

        canvasGroup.alpha = 1f;

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(AnimatePopup(localPosition));
    }

    private IEnumerator AnimatePopup(Vector2 startPosition)
    {
        Vector2 endPosition = startPosition + new Vector2(0f, moveUpDistance);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            if (popupRect != null)
                popupRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (popupRoot != null)
            popupRoot.SetActive(false);

        popupRoutine = null;
    }
}