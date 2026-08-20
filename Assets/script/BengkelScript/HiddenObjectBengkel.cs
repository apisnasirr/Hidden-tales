using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HiddenObjectBengkel : MonoBehaviour
{
    [Header("Object Info")]
    [SerializeField] private string categoryId;

    [Header("References")]
    [SerializeField] private ManagerHiddenObjectBengkel manager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D col2D;
    [SerializeField] private Collider col3D;

    [Header("UI Fly Copy")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform flyParent;
    [SerializeField] private Vector2 flyImageSize = new Vector2(70f, 70f);
    [SerializeField] private bool preserveSpriteAspect = true;

    [Header("Move To Center")]
    [SerializeField] private float moveToCenterDuration = 0.35f;
    [SerializeField] private float centerSpacing = 0.45f;

    [Header("Move To UI")]
    [SerializeField] private float moveToUIDuration = 0.45f;
    [SerializeField] private float uiFlyStartScale = 1f;
    [SerializeField] private float uiFlyEndScale = 0.55f;

    [Header("Optional SFX")]
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private bool playClickSfx = true;

    [Header("Hint Animation")]
    [SerializeField] private float hintScale = 1.18f;
    [SerializeField] private float hintDuration = 0.12f;
    [SerializeField] private int hintPulseCount = 2;

    private bool isFound = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private AudioSource audioSource;

    public string CategoryId => categoryId;
    public bool IsFound => isFound;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (manager == null)
            manager = FindObjectOfType<ManagerHiddenObjectBengkel>(true);

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (col2D == null)
            col2D = GetComponent<Collider2D>();

        if (col3D == null)
            col3D = GetComponent<Collider>();

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas != null && flyParent == null)
            flyParent = targetCanvas.transform as RectTransform;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (manager == null)
            manager = FindObjectOfType<ManagerHiddenObjectBengkel>(true);

        if (manager != null)
            manager.RegisterObject(this);
    }

    private void OnDestroy()
    {
        if (manager != null)
            manager.UnregisterObject(this);
    }

    private void OnMouseDown()
    {
        if (isFound || isAnimating)
            return;

        CollectByClick();
    }

    private void CollectByClick()
    {
        isFound = true;
        DisableCollider();

        PlayClickSound();

        if (manager == null)
            manager = FindObjectOfType<ManagerHiddenObjectBengkel>(true);

        if (manager != null)
        {
            manager.TryMarkFound(this, true);
        }
        else
        {
            Debug.LogWarning("[HiddenObjectBengkel] ManagerHiddenObjectBengkel tak jumpa.");
            StartCoroutine(CollectWithoutManagerRoutine());
        }
    }

    private void PlayClickSound()
    {
        if (!playClickSfx)
            return;

        if (clickSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSfx);
            return;
        }

        // Kalau SFXManager awak ada PlayCoinGain(), boleh guna ini sebagai bunyi sementara.
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayCoinGain();
    }

    private IEnumerator CollectWithoutManagerRoutine()
    {
        yield return PlayMagnetMoveToCenter(0, 1);
        HideWorldVisual();
    }

    public bool BeginMagnetSelection()
    {
        if (isFound || isAnimating)
            return false;

        isFound = true;
        DisableCollider();

        return true;
    }

    public IEnumerator PlayMagnetMoveToCenter(int index, int totalTargets)
    {
        isAnimating = true;

        Camera cam = Camera.main;

        if (cam == null)
        {
            isAnimating = false;
            yield break;
        }

        Vector3 startPosition = transform.position;
        Vector3 centerPosition = GetCameraCenterWorldPosition(cam);

        if (totalTargets > 1)
        {
            float offsetIndex = index - ((totalTargets - 1) * 0.5f);
            centerPosition += new Vector3(offsetIndex * centerSpacing, 0f, 0f);
        }

        centerPosition.z = startPosition.z;

        float timer = 0f;

        while (timer < moveToCenterDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / moveToCenterDuration);
            t = SmoothEase(t);

            transform.position = Vector3.Lerp(startPosition, centerPosition, t);

            yield return null;
        }

        transform.position = centerPosition;
        isAnimating = false;
    }

    public IEnumerator PlayMagnetMoveToUI()
    {
        isAnimating = true;

        if (manager == null)
            manager = FindObjectOfType<ManagerHiddenObjectBengkel>(true);

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas != null && flyParent == null)
            flyParent = targetCanvas.transform as RectTransform;

        if (manager == null || targetCanvas == null || flyParent == null || spriteRenderer == null)
        {
            HideWorldVisual();
            isAnimating = false;

            if (manager != null)
                manager.TryMarkFound(this, false);

            yield break;
        }

        Camera mainCamera = Camera.main;
        Camera uiCamera = GetUICamera(targetCanvas);

        Vector3 startScreenPosition;

        if (mainCamera != null)
            startScreenPosition = mainCamera.WorldToScreenPoint(transform.position);
        else
            startScreenPosition = transform.position;

        Vector3 targetScreenPosition = manager.GetTargetUIScreenPosition(this, uiCamera);

        if (targetScreenPosition == Vector3.zero)
            targetScreenPosition = startScreenPosition;

        Vector2 startLocalPosition = ScreenToCanvasLocal(startScreenPosition, targetCanvas, flyParent);
        Vector2 targetLocalPosition = ScreenToCanvasLocal(targetScreenPosition, targetCanvas, flyParent);

        GameObject flyObject = CreateFlyImageObject(startLocalPosition);

        HideWorldVisual();

        if (flyObject == null)
        {
            isAnimating = false;
            manager.TryMarkFound(this, false);
            yield break;
        }

        RectTransform flyRect = flyObject.GetComponent<RectTransform>();

        float timer = 0f;

        while (timer < moveToUIDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / moveToUIDuration);
            float easedT = SmoothEase(t);

            flyRect.anchoredPosition = Vector2.Lerp(startLocalPosition, targetLocalPosition, easedT);

            float scale = Mathf.Lerp(uiFlyStartScale, uiFlyEndScale, easedT);
            flyRect.localScale = Vector3.one * scale;

            yield return null;
        }

        flyRect.anchoredPosition = targetLocalPosition;

        Destroy(flyObject);

        isAnimating = false;

        // Untuk kes magnet. Kalau object sudah didaftarkan oleh manager, call ini akan diabaikan.
        manager.TryMarkFound(this, false);
    }

    private GameObject CreateFlyImageObject(Vector2 startLocalPosition)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return null;

        GameObject flyObject = new GameObject("Flying Item UI - " + gameObject.name);
        flyObject.transform.SetParent(flyParent, false);
        flyObject.transform.SetAsLastSibling();

        RectTransform rect = flyObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = startLocalPosition;
        rect.localScale = Vector3.one * uiFlyStartScale;
        rect.sizeDelta = flyImageSize;

        Image image = flyObject.AddComponent<Image>();
        image.sprite = spriteRenderer.sprite;
        image.color = spriteRenderer.color;
        image.raycastTarget = false;
        image.preserveAspect = preserveSpriteAspect;

        return flyObject;
    }

    private Vector3 GetCameraCenterWorldPosition(Camera cam)
    {
        float zDistance = Mathf.Abs(transform.position.z - cam.transform.position.z);

        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, zDistance));
        center.z = transform.position.z;

        return center;
    }

    private Vector2 ScreenToCanvasLocal(Vector3 screenPosition, Canvas canvas, RectTransform parentRect)
    {
        Camera uiCamera = GetUICamera(canvas);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition
        );

        return localPosition;
    }

    private Camera GetUICamera(Canvas canvas)
    {
        if (canvas == null)
            return null;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    public void PlayHint()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StopCoroutine(nameof(HintRoutine));
        StartCoroutine(nameof(HintRoutine));
    }

    private IEnumerator HintRoutine()
    {
        Vector3 normalScale = originalScale;
        Vector3 biggerScale = originalScale * hintScale;

        for (int i = 0; i < hintPulseCount; i++)
        {
            float timer = 0f;

            while (timer < hintDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(timer / hintDuration);
                transform.localScale = Vector3.Lerp(normalScale, biggerScale, t);

                yield return null;
            }

            timer = 0f;

            while (timer < hintDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(timer / hintDuration);
                transform.localScale = Vector3.Lerp(biggerScale, normalScale, t);

                yield return null;
            }
        }

        transform.localScale = normalScale;
    }

    private void DisableCollider()
    {
        if (col2D != null)
            col2D.enabled = false;

        if (col3D != null)
            col3D.enabled = false;
    }

    private void HideWorldVisual()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;
    }

    private float SmoothEase(float t)
    {
        return t * t * (3f - 2f * t);
    }

    public void ResetHiddenObject()
    {
        isFound = false;
        isAnimating = false;

        transform.localScale = originalScale;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = true;

        if (col2D != null)
            col2D.enabled = true;

        if (col3D != null)
            col3D.enabled = true;
    }
}