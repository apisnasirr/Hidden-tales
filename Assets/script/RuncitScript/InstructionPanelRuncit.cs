using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;              

public class InstructionPanelRuncit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private CanvasGroup instructionCanvasGroup;

    [Header("Swipe Navigation")]
    [SerializeField] private RectTransform contentContainer; 
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private int totalPages = 3;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine fadeRoutine;
    private int currentPage = 0;
    private float pageWidth;
    private Vector2 startDragPosition;

    private void Awake()
    {
        if (instructionPanel != null && instructionCanvasGroup == null)
            instructionCanvasGroup = instructionPanel.GetComponent<CanvasGroup>();

        if (instructionPanel != null && instructionCanvasGroup == null)
            instructionCanvasGroup = instructionPanel.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (contentContainer != null && contentContainer.parent != null)
        {
            pageWidth = contentContainer.parent.GetComponent<RectTransform>().rect.width;
        }

        OpenInstruction();
    }

    public void OpenInstruction()
    {
        PlayButtonSFX();

        currentPage = 0;
        UpdatePageDisplay(0f); 

        FadeInPanel();
    }

    public void CloseInstruction()
    {
        PlayButtonSFX();
        FadeOutPanel();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float swipeDistance = eventData.position.x - startDragPosition.x;

        if (Mathf.Abs(swipeDistance) > 50f)
        {
            if (swipeDistance > 0 && currentPage > 0)
            {
                currentPage--;
                UpdatePageDisplay(slideDuration);
            }
            else if (swipeDistance < 0 && currentPage < totalPages - 1)
            {
                currentPage++;
                UpdatePageDisplay(slideDuration);
            }
        }
    }

    private void UpdatePageDisplay(float duration)
    {
        if (contentContainer == null) return;

        float targetX = -(currentPage * pageWidth);
        contentContainer.DOAnchorPosX(targetX, duration).SetEase(Ease.OutQuint).SetUpdate(true);
    }

    private void FadeOutPanel()
    {
        if (instructionPanel == null || instructionCanvasGroup == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeCanvasGroup(instructionCanvasGroup.alpha, 0f, false));
    }

    private void FadeInPanel()
    {
        if (instructionPanel == null || instructionCanvasGroup == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        instructionPanel.SetActive(true);
        fadeRoutine = StartCoroutine(FadeCanvasGroup(instructionCanvasGroup.alpha, 1f, true));
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, bool keepActive)
    {
        instructionCanvasGroup.alpha = startAlpha;

        if (endAlpha > 0f)
        {
            instructionCanvasGroup.interactable = true;
            instructionCanvasGroup.blocksRaycasts = true;
        }

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            instructionCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        instructionCanvasGroup.alpha = endAlpha;

        if (!keepActive && instructionPanel != null)
        {
            instructionCanvasGroup.interactable = false;
            instructionCanvasGroup.blocksRaycasts = false;
            instructionPanel.SetActive(false);
        }

        fadeRoutine = null;
    }

    private void PlayButtonSFX()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }
}