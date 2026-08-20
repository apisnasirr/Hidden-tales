using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class WrongClickDetectorRuncit : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ShopManager shopManager;

    [Header("Wrong Click Setting")]
    [SerializeField] private bool enableWrongClickDetection = true;

    [Tooltip("Layer untuk object yang betul / hidden object.")]
    [SerializeField] private LayerMask correctObjectLayer = ~0;

    [Tooltip("Kalau hidden object ada tag tertentu, letak tag dekat sini. Kalau tak nak check tag, kosongkan sahaja.")]
    [SerializeField] private string correctObjectTag = "";

    [Header("Event Bila Salah Klik")]
    [SerializeField] private UnityEvent onWrongClick;

    private bool validClickRegisteredThisFrame = false;
    private Coroutine checkClickCoroutine;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!enableWrongClickDetection)
            return;

        if (shopManager != null && shopManager.IsShopOpen)
            return;

#if UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#else
        HandleMouseInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (IsPointerOverUI())
            return;

        StartCheckClickAfterFrame(Input.mousePosition);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        if (IsPointerOverUI(touch.fingerId))
            return;

        StartCheckClickAfterFrame(touch.position);
    }

    private void StartCheckClickAfterFrame(Vector2 screenPosition)
    {
        if (checkClickCoroutine != null)
            StopCoroutine(checkClickCoroutine);

        checkClickCoroutine = StartCoroutine(CheckClickAfterFrame(screenPosition));
    }

    private IEnumerator CheckClickAfterFrame(Vector2 screenPosition)
    {
        validClickRegisteredThisFrame = false;

        // Tunggu sampai hujung frame supaya script HiddenObject sempat panggil RegisterValidClick()
        yield return new WaitForEndOfFrame();

        if (validClickRegisteredThisFrame)
        {
            validClickRegisteredThisFrame = false;
            checkClickCoroutine = null;
            yield break;
        }

        CheckWorldClick(screenPosition);

        validClickRegisteredThisFrame = false;
        checkClickCoroutine = null;
    }

    public void RegisterValidClick()
    {
        validClickRegisteredThisFrame = true;
        Debug.Log("[WrongClickDetectorRuncit] Valid click registered.");
    }

    private void CheckWorldClick(Vector2 screenPosition)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[WrongClickDetectorRuncit] Main Camera tak jumpa.");
            return;
        }

        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        RaycastHit2D hit = Physics2D.Raycast(
            worldPosition,
            Vector2.zero,
            0f,
            correctObjectLayer
        );

        if (hit.collider != null)
        {
            if (IsCorrectObject(hit.collider.gameObject))
            {
                Debug.Log("[WrongClickDetectorRuncit] Klik object betul melalui raycast: " + hit.collider.name);
                return;
            }
        }

        HandleWrongClick();
    }

    private bool IsCorrectObject(GameObject clickedObject)
    {
        if (clickedObject == null)
            return false;

        if (string.IsNullOrEmpty(correctObjectTag))
            return true;

        return clickedObject.CompareTag(correctObjectTag);
    }

    private void HandleWrongClick()
    {
        Debug.Log("[WrongClickDetectorRuncit] Salah klik.");

        if (onWrongClick != null)
            onWrongClick.Invoke();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerOverUI(int fingerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    public void EnableWrongClickDetection()
    {
        enableWrongClickDetection = true;
    }

    public void DisableWrongClickDetection()
    {
        enableWrongClickDetection = false;
    }
}