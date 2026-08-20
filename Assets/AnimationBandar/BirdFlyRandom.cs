using UnityEngine;

public class BirdFlyRandom2D : MonoBehaviour
{
    public Transform topLeft;
    public Transform bottomRight;

    public float minSpeed = 0.5f;
    public float maxSpeed = 1.2f;
    public float reachDistance = 0.1f;
    public float waitTime = 0.3f;

    private Vector3 targetPos;
    private float speed;
    private float waitTimer;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        speed = Random.Range(minSpeed, maxSpeed);
        PickNewTarget();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        if (sr != null)
        {
            if (targetPos.x > transform.position.x)
                sr.flipX = false;
            else if (targetPos.x < transform.position.x)
                sr.flipX = true;
        }

        if (Vector2.Distance(transform.position, targetPos) <= reachDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                speed = Random.Range(minSpeed, maxSpeed);
                PickNewTarget();
            }
        }
    }

    void PickNewTarget()
    {
        float minX = Mathf.Min(topLeft.position.x, bottomRight.position.x);
        float maxX = Mathf.Max(topLeft.position.x, bottomRight.position.x);
        float minY = Mathf.Min(bottomRight.position.y, topLeft.position.y);
        float maxY = Mathf.Max(bottomRight.position.y, topLeft.position.y);

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        targetPos = new Vector3(randomX, randomY, transform.position.z);
    }
}