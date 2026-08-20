using UnityEngine;
using System.Collections;

public class HiddenCharacterSequence : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    public string fullSequenceAnim = "RevealSequence";

    [Header("Idle Animation")]
    [SerializeField] private string idleBlinkAnim = "IdleBlink";

    [Header("Movement")]
    public Vector2 moveDirection = Vector2.right;
    public float moveSpeed = 2f;
    public float moveDuration = 1f;

    [Header("Dialog")]
    [SerializeField] private HiddenCharacterDialog hiddenCharacterDialog;

    private bool sudahKlik = false;
    private bool sedangMain = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (hiddenCharacterDialog == null)
            hiddenCharacterDialog = GetComponent<HiddenCharacterDialog>();
    }

    private void OnMouseDown()
    {
        if (sudahKlik || sedangMain)
            return;

        sudahKlik = true;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        sedangMain = true;

        if (animator != null && !string.IsNullOrEmpty(fullSequenceAnim))
            animator.Play(fullSequenceAnim, 0, 0f);

        float timer = 0f;

        while (timer < moveDuration)
        {
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Lepas character berhenti, tukar ke idle kelip mata
        if (animator != null && !string.IsNullOrEmpty(idleBlinkAnim))
            animator.Play(idleBlinkAnim, 0, 0f);

        if (hiddenCharacterDialog != null)
            hiddenCharacterDialog.ShowDialog();

        sedangMain = false;
    }
}