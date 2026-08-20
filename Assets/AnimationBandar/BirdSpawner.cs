using UnityEngine;

public class BirdSpawner2D : MonoBehaviour
{
    public GameObject birdPrefab;
    public Transform topLeft;
    public Transform bottomRight;
    public int birdCount = 5;

    void Start()
    {
        if (birdPrefab == null || topLeft == null || bottomRight == null)
        {
            Debug.LogError("BirdSpawner2D: assign birdPrefab, topLeft, dan bottomRight dulu.");
            return;
        }

        float minX = Mathf.Min(topLeft.position.x, bottomRight.position.x);
        float maxX = Mathf.Max(topLeft.position.x, bottomRight.position.x);
        float minY = Mathf.Min(bottomRight.position.y, topLeft.position.y);
        float maxY = Mathf.Max(bottomRight.position.y, topLeft.position.y);

        for (int i = 0; i < birdCount; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);

            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            GameObject bird = Instantiate(birdPrefab, spawnPos, Quaternion.identity);

            BirdFlyRandom2D birdScript = bird.GetComponent<BirdFlyRandom2D>();
            if (birdScript != null)
            {
                birdScript.topLeft = topLeft;
                birdScript.bottomRight = bottomRight;
            }
        }
    }
}