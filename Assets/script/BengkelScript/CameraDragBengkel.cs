using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDrag2DBengkel : MonoBehaviour
{
    [Header("World Bounds")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    [Header("Scene Plane")]
    [SerializeField] private float sceneZ = 0f;

    [Header("Drag Feel")]
    [SerializeField] private float dragSensitivity = 1f;
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float dragDeadZone = 0.03f;

    [Header("Hint Zoom")]
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomSmoothTime = 0.08f;

    [SerializeField] private float mouseZoomSpeed = 2f;
    [SerializeField] private float pinchZoomSpeed = 0.01f;
    [SerializeField] private bool allowManualZoom = true;

    private Camera cam;
    private bool isDragging;
    private Vector3 dragStartWorld;
    private Vector3 camStartPos;

    private Vector3 targetCameraPos;
    private Vector3 currentVelocity = Vector3.zero;

    private float targetZoom;
    private float zoomVelocity;
    private float defaultZoom;

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetCameraPos = transform.position;

        defaultZoom = cam.orthographicSize;
        targetZoom = defaultZoom;
    }

    private void Update()
    {
        if (allowManualZoom)
        {
            HandleMouseZoom();
        }

        if (Input.touchCount == 2)
        {
            if (allowManualZoom)
                HandlePinchZoom();

            isDragging = false;
            return;
        }

        if (Input.touchCount > 0)
        {
            HandleTouch();
            return;
        }

        HandleMouse();
    }

    private void LateUpdate()
    {
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );

        targetCameraPos = ClampCameraPosition(targetCameraPos);

        Vector3 newPos = Vector3.SmoothDamp(
            transform.position,
            targetCameraPos,
            ref currentVelocity,
            smoothTime,
            maxSpeed
        );

        newPos.z = transform.position.z;
        transform.position = ClampCameraPosition(newPos);
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            DragTo(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void HandleTouch()
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginDrag(touch.position);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isDragging)
                    DragTo(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndDrag();
                break;
        }
    }

    private void HandleMouseZoom()
{
    if (Input.touchCount > 0)
        return;

    if (IsPointerOverUI())
        return;

    float scroll = Input.mouseScrollDelta.y;
    if (Mathf.Abs(scroll) < 0.01f)
        return;

    targetZoom -= scroll * mouseZoomSpeed;
    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
}

    private void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject(touch0.fingerId) ||
                EventSystem.current.IsPointerOverGameObject(touch1.fingerId))
            {
                return;
            }
        }

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currentMagnitude = (touch0.position - touch1.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;

        targetZoom -= difference * pinchZoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    private void BeginDrag(Vector2 screenPos)
    {
        if (IsPointerOverUI())
        {
            isDragging = false;
            return;
        }

        isDragging = true;
        dragStartWorld = ScreenToWorldOnScenePlane(screenPos);
        camStartPos = targetCameraPos;
        currentVelocity = Vector3.zero;
    }

    private void DragTo(Vector2 screenPos)
    {
        Vector3 currentWorld = ScreenToWorldOnScenePlane(screenPos);
        Vector3 delta = dragStartWorld - currentWorld;

        if (delta.magnitude < dragDeadZone)
            return;

        delta *= dragSensitivity;

        Vector3 wantedPos = camStartPos + delta;
        wantedPos.z = transform.position.z;

        targetCameraPos = ClampCameraPosition(wantedPos);
    }

    private void EndDrag()
    {
        isDragging = false;
    }

    public void FocusOnWorldPosition(Vector3 worldPos)
    {
        isDragging = false;
        currentVelocity = Vector3.zero;

        Vector3 wantedPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        targetCameraPos = ClampCameraPosition(wantedPos);
    }

    public void SetZoomTarget(float zoomSize)
    {
        targetZoom = Mathf.Clamp(zoomSize, minZoom, maxZoom);
        zoomVelocity = 0f;
    }

    public void ResetZoom()
    {
        targetZoom = Mathf.Clamp(defaultZoom, minZoom, maxZoom);
        zoomVelocity = 0f;
    }

    private Vector3 ScreenToWorldOnScenePlane(Vector2 screenPos)
    {
        float distanceFromCamera = Mathf.Abs(sceneZ - cam.transform.position.z);
        Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, distanceFromCamera);
        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);
        worldPoint.z = sceneZ;
        return worldPoint;
    }

    private Vector3 ClampCameraPosition(Vector3 targetPos)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minCamX = minX + halfWidth;
        float maxCamX = maxX - halfWidth;
        float minCamY = minY + halfHeight;
        float maxCamY = maxY - halfHeight;

        float clampedX = (minCamX > maxCamX)
            ? (minX + maxX) * 0.5f
            : Mathf.Clamp(targetPos.x, minCamX, maxCamX);

        float clampedY = (minCamY > maxCamY)
            ? (minY + maxY) * 0.5f
            : Mathf.Clamp(targetPos.y, minCamY, maxCamY);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
}