using UnityEngine;

public class CharacterIntro : MonoBehaviour
{
    public float moveDistance = 1.5f;   // jauh mana nak bergerak
    public float moveSpeed = 2f;        // laju jalan
    public Animator animator;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isMoving = true;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.right * moveDistance;

        animator.Play("Walk");
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                isMoving = false;
                animator.Play("Wave");
            }
        }
    }
}