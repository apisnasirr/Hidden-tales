using UnityEngine;
using System.Collections;

public class HiddenCharacterReveal : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 2f;
    public float moveSpeed = 2f;
    public Vector2 moveDirection = Vector2.right;

    [Header("Animation")]
    public Animator animator;
    public string walkAnim = "Walk";
    public string waveAnim = "Wave";
    public string idleAnim = "Idle";

    [Header("Settings")]
    public bool canClick = true;

    private bool isRevealed = false;
    private bool isMoving = false;
    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position + (Vector3)(moveDirection.normalized * moveDistance);
    }

    void OnMouseDown()
    {
        if (!canClick) return;
        if (isRevealed) return;

        isRevealed = true;
        StartCoroutine(RevealSequence());
    }

    IEnumerator RevealSequence()
    {
        isMoving = true;

        if (animator != null)
            animator.Play(walkAnim);

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        if (animator != null)
            animator.Play(waveAnim);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(waveAnim))
            yield return null;

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (animator != null)
            animator.Play(idleAnim);
    }
}