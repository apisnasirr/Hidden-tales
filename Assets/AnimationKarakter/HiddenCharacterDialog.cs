using System.Collections;
using TMPro;
using UnityEngine;

public class HiddenCharacterDialog : MonoBehaviour
{
    [Header("Dialog UI")]
    [SerializeField] private GameObject dialogBubble;
    [SerializeField] private TMP_Text dialogText;

    [Header("Dialog Settings")]
    [SerializeField] private string message = "Terima kasih!";
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float delayBeforeShow = 0.2f;

    private Coroutine dialogRoutine;

    private void Awake()
    {
        HideDialog();
    }

    private void Start()
    {
        HideDialog();
    }

    private void OnEnable()
    {
        HideDialog();
    }

    public void ShowDialog()
    {
        if (dialogRoutine != null)
            StopCoroutine(dialogRoutine);

        dialogRoutine = StartCoroutine(ShowDialogRoutine());
    }

    private IEnumerator ShowDialogRoutine()
    {
        if (delayBeforeShow > 0f)
            yield return new WaitForSeconds(delayBeforeShow);

        if (dialogText != null)
            dialogText.text = message;

        if (dialogBubble != null)
            dialogBubble.SetActive(true);

        yield return new WaitForSeconds(showDuration);

        HideDialog();

        dialogRoutine = null;
    }

    public void HideDialog()
    {
        if (dialogBubble != null)
            dialogBubble.SetActive(false);
    }
}