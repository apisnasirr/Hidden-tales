using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinPopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image coinImage;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float moveUpDistance = 60f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Play(int amount)
    {
        if (amountText != null)
            amountText.text = "+" + amount;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, moveUpDistance);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}