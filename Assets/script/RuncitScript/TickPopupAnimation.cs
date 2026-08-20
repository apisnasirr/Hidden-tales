using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TickPopupAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float startScale = 0f;
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float normalScale = 1f;

    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float settleDuration = 0.10f;

    [Header("Setting")]
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private bool ignoreLayout = true;

    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement;
    private Coroutine popupRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (ignoreLayout)
        {
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = gameObject.AddComponent<LayoutElement>();

            layoutElement.ignoreLayout = true;
        }

        // Jangan SetActive(false), sebab itu boleh kacau ScrollRect/LayoutGroup.
        gameObject.SetActive(true);

        if (hideOnAwake)
            HideTick();
        else
            ShowWithoutAnimation();
    }

    public void Play()
    {
        // Pastikan object kekal active. Jangan guna SetActive(true/false) untuk animation.
        gameObject.SetActive(true);

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(PlayPopupAnimation());
    }

    private IEnumerator PlayPopupAnimation()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        transform.localScale = Vector3.one * startScale;

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / popDuration);
            float scale = Mathf.Lerp(startScale, popScale, t);

            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        timer = 0f;

        while (timer < settleDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / settleDuration);
            float scale = Mathf.Lerp(popScale, normalScale, t);

            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        transform.localScale = Vector3.one * normalScale;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        popupRoutine = null;
    }

    public void HideTick()
    {
        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
            popupRoutine = null;
        }

        // Jangan SetActive(false)
        transform.localScale = Vector3.zero;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowWithoutAnimation()
    {
        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
            popupRoutine = null;
        }

        // Jangan SetActive(true/false) untuk layout
        gameObject.SetActive(true);
        transform.localScale = Vector3.one * normalScale;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}