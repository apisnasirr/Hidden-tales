using UnityEngine;

public class CameraBoundaries : MonoBehaviour
{
    [Header("Boundaries")]
    public float minX, maxX, minY, maxY;

    [Header("Optional")]
    public bool applyZoomLimit = true;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 currentPosition = transform.position;

        // Get camera size at the current zoom level
        if (applyZoomLimit)
        {
            float cameraHeight = mainCamera.orthographicSize * 2;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Clamp the camera's position to stay within the game area
            currentPosition.x = Mathf.Clamp(currentPosition.x, minX + cameraWidth / 2, maxX - cameraWidth / 2);
            currentPosition.y = Mathf.Clamp(currentPosition.y, minY + cameraHeight / 2, maxY - cameraHeight / 2);
        }
        else
        {
            currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
            currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);
        }

        transform.position = currentPosition;
    }
}