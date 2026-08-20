using System.Collections;
using UnityEngine;

public class InstructionPanelRuncit : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private CanvasGroup instructionCanvasGroup;

    [Header("Timing")]
    [SerializeField] private float hideAfterDragDelay = 5f;
    [SerializeField] private float showAfterIdleDelay = 5f;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Drag Detection")]
    [SerializeField] private float dragThreshold = 2f;

    private Vector3 dragStartMousePosition;
    private bool isPressing = false;
    private bool hasDraggedAtLeastOnce = false;
    private bool hideTimerStarted = false;

    private float idleTimer = 0f;

    private Coroutine hideRoutine;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (instructionPanel != null && instructionCanvasGroup == null)
            instructionCanvasGroup = instructionPanel.GetComponent<CanvasGroup>();

        if (instructionPanel != null && instructionCanvasGroup == null)
            instructionCanvasGroup = instructionPanel.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 0f;
            instructionCanvasGroup.interactable = false;
            instructionCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartMousePosition = Input.mousePosition;
            isPressing = true;
        }

        if (Input.GetMouseButton(0) && isPressing)
        {
            float dragDistance = Vector3.Distance(dragStartMousePosition, Input.mousePosition);

            if (dragDistance > dragThreshold)
            {
                hasDraggedAtLeastOnce = true;
                idleTimer = 0f;

                if (!hideTimerStarted)
                {
                    hideTimerStarted = true;

                    if (hideRoutine != null)
                        StopCoroutine(hideRoutine);

                    hideRoutine = StartCoroutine(HideAfterDelay(hideAfterDragDelay));
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPressing = false;
        }

        if (hasDraggedAtLeastOnce && !Input.GetMouseButton(0))
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= showAfterIdleDelay)
            {
                FadeInPanel();
                idleTimer = 0f;
                hasDraggedAtLeastOnce = false;
                hideTimerStarted = false;
            }
        }
    }

    public void ShowInstructionNow()
    {
        idleTimer = 0f;
        hasDraggedAtLeastOnce = false;
        hideTimerStarted = false;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        FadeInPanel();
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        FadeOutPanel();
        hideRoutine = null;
    }

    private void FadeOutPanel()
    {
        if (instructionPanel == null || instructionCanvasGroup == null)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeCanvasGroup(instructionCanvasGroup.alpha, 0f, false));
    }

    private void FadeInPanel()
    {
        if (instructionPanel == null || instructionCanvasGroup == null)
            return;

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
}