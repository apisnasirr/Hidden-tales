using System.Collections;
using UnityEngine;

public class InstructionPanelBengkel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private CanvasGroup instructionCanvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.4f;

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
        // Automatically pop up when the level loads!
        OpenInstruction();
    }

    // Link this to the '?' button on the top right
    public void OpenInstruction()
    {
        PlayButtonSFX();
        FadeInPanel();
    }

    // Link this to the 'X' button on the Panduan panel
    public void CloseInstruction()
    {
        PlayButtonSFX();
        FadeOutPanel();
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