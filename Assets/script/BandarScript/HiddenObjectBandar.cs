using System.Collections;
using UnityEngine;

public class HiddenObjectBandar : MonoBehaviour
{
    [Header("Object Info")]
    [SerializeField] private string categoryId;
    [SerializeField] private ManagerHiddenObjectBandar manager;

    [Header("Camera / Canvas")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas uiCanvas;

    [Header("Animation")]
    [SerializeField, Range(0f, 1f)] private float centerViewportX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float centerViewportY = 0.5f;
    [SerializeField] private float moveToCenterDuration = 0.35f;
    [SerializeField] private float stayDuration = 0.35f;
    [SerializeField] private float moveToUIDuration = 0.45f;
    [SerializeField] private float scaleUpMultiplier = 2f;
    [SerializeField] private float scaleToUIRatio = 0.8f;

    [Header("Hint")]
    [SerializeField] private float hintDuration = 1.2f;
    [SerializeField] private float hintPulseSpeed = 3f;
    [SerializeField] private float hintScaleMultiplier = 1.2f;
    [SerializeField] private Color hintColor = Color.yellow;

    [Header("Magnet")]
    [SerializeField] private float magnetMoveToCenterDuration = 0.3f;
    [SerializeField] private float magnetCenterStayDuration = 0.2f;
    [SerializeField, Range(0.02f, 0.3f)] private float magnetCenterViewportSpacing = 0.12f;
    [SerializeField] private float magnetMoveToUIDuration = 0.35f;
    [SerializeField] private float magnetScaleMultiplier = 1.15f;

    [Header("Bandar Special SFX")]
    [SerializeField] private bool playCatSfx = false;
    [SerializeField] private bool playBirdSfx = false;
    [SerializeField] private bool playFrogSfx = false;

    private bool isFound = false;
    private Collider col3D;
    private Collider2D col2D;
    private SpriteRenderer spriteRenderer;

    private Vector3 defaultScale;
    private Color defaultColor;
    private Coroutine hintRoutine;

    public bool IsFound => isFound;
    public string CategoryId => categoryId;
    public float MagnetCenterPhaseDuration => magnetMoveToCenterDuration + magnetCenterStayDuration;

    private void Awake()
    {
        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        RefreshReferences();

        defaultScale = transform.localScale;
        defaultColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    private void OnEnable()
    {
        RefreshReferences();

        if (manager != null)
            manager.RegisterObject(this);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.UnregisterObject(this);
    }

    private void OnMouseDown()
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return;

        HandleCorrectClick();
    }

    public void RefreshAfterSceneLoad()
    {
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (manager == null)
            manager = FindObjectOfType<ManagerHiddenObjectBandar>(true);

        if (uiCanvas == null)
            uiCanvas = FindObjectOfType<Canvas>(true);
    }

    public bool HandleCorrectClick()
    {
        RefreshReferences();

        if (isFound) return false;
        if (manager == null) return false;
        if (worldCamera == null) return false;
        if (string.IsNullOrEmpty(categoryId)) return false;

        StopHintVisual();

        manager.RegisterObject(this);
        bool accepted = manager.TryMarkFound(this);

        if (!accepted)
            return false;

        isFound = true;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCorrectClick();

            if (playCatSfx)
                SFXManager.Instance.PlayCat();

            if (playBirdSfx)
                SFXManager.Instance.PlayBird();

            if (playFrogSfx)
                SFXManager.Instance.PlayFrog();
        }

        if (col3D != null) col3D.enabled = false;
        if (col2D != null) col2D.enabled = false;

        StartCoroutine(PlayFoundAnimation());
        return true;
    }

    public void PlayHint()
    {
        if (isFound) return;

        StopHintVisual();
        hintRoutine = StartCoroutine(HintRoutine());
    }

    public bool BeginMagnetSelection()
    {
        RefreshReferences();

        if (isFound) return false;
        if (manager == null) return false;
        if (worldCamera == null) return false;
        if (string.IsNullOrEmpty(categoryId)) return false;

        StopHintVisual();

        manager.RegisterObject(this);
        bool accepted = manager.TryMarkFound(this);

        if (!accepted) return false;

        isFound = true;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCorrectClick();

            if (playCatSfx)
                SFXManager.Instance.PlayCat();

            if (playBirdSfx)
                SFXManager.Instance.PlayBird();

            if (playFrogSfx)
                SFXManager.Instance.PlayFrog();
        }

        if (col3D != null) col3D.enabled = false;
        if (col2D != null) col2D.enabled = false;

        return true;
    }

    public IEnumerator PlayMagnetMoveToCenter(int magnetIndex, int totalTargets)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        Vector3 centerPos = GetMagnetCenterWorldTarget(magnetIndex, totalTargets);
        Vector3 centerScale = defaultScale * magnetScaleMultiplier;

        float time = 0f;

        while (time < magnetMoveToCenterDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / magnetMoveToCenterDuration);

            transform.position = Vector3.Lerp(startPos, centerPos, t);
            transform.localScale = Vector3.Lerp(startScale, centerScale, t);

            yield return null;
        }

        transform.position = centerPos;
        transform.localScale = centerScale;

        if (magnetCenterStayDuration > 0f)
            yield return new WaitForSeconds(magnetCenterStayDuration);
    }

    public IEnumerator PlayMagnetMoveToUI()
    {
        manager.CenterTargetUI(categoryId);

        Canvas.ForceUpdateCanvases();
        yield return null;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = defaultScale * scaleToUIRatio;

        float time = 0f;

        while (time < magnetMoveToUIDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / magnetMoveToUIDuration);

            Vector3 uiWorldTarget = GetCurrentUIWorldTarget();

            transform.position = Vector3.Lerp(startPos, uiWorldTarget, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        transform.position = GetCurrentUIWorldTarget();
        transform.localScale = endScale;

        manager.CompleteToUI(categoryId);

        gameObject.SetActive(false);
    }

    private Vector3 GetMagnetCenterWorldTarget(int magnetIndex, int totalTargets)
    {
        totalTargets = Mathf.Max(1, totalTargets);

        float slotOffset = magnetIndex - (totalTargets - 1) * 0.5f;
        float targetViewportX = centerViewportX + slotOffset * magnetCenterViewportSpacing;
        targetViewportX = Mathf.Clamp(targetViewportX, 0.1f, 0.9f);

        return GetViewportWorldPosition(targetViewportX, centerViewportY);
    }

    private IEnumerator HintRoutine()
    {
        float time = 0f;

        while (time < hintDuration)
        {
            time += Time.deltaTime;
            float pulse = Mathf.PingPong(time * hintPulseSpeed, 1f);

            transform.localScale = Vector3.Lerp(defaultScale, defaultScale * hintScaleMultiplier, pulse);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(defaultColor, hintColor, pulse);

            yield return null;
        }

        transform.localScale = defaultScale;

        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;

        hintRoutine = null;
    }

    private void StopHintVisual()
    {
        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        transform.localScale = defaultScale;

        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }

    private IEnumerator PlayFoundAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = defaultScale;

        Vector3 centerPos = GetViewportWorldPosition(centerViewportX, centerViewportY);
        Vector3 bigScale = startScale * scaleUpMultiplier;

        float time = 0f;

        while (time < moveToCenterDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveToCenterDuration);

            transform.position = Vector3.Lerp(startPos, centerPos, t);
            transform.localScale = Vector3.Lerp(startScale, bigScale, t);

            yield return null;
        }

        transform.position = centerPos;
        transform.localScale = bigScale;

        yield return new WaitForSeconds(stayDuration);

        manager.CenterTargetUI(categoryId);

        Canvas.ForceUpdateCanvases();
        yield return null;

        time = 0f;
        Vector3 flyStart = transform.position;
        Vector3 flyStartScale = transform.localScale;
        Vector3 flyEndScale = defaultScale * scaleToUIRatio;

        while (time < moveToUIDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveToUIDuration);

            Vector3 uiWorldTarget = GetCurrentUIWorldTarget();

            transform.position = Vector3.Lerp(flyStart, uiWorldTarget, t);
            transform.localScale = Vector3.Lerp(flyStartScale, flyEndScale, t);

            yield return null;
        }

        transform.position = GetCurrentUIWorldTarget();
        transform.localScale = flyEndScale;

        manager.CompleteToUI(categoryId);

        gameObject.SetActive(false);
    }

    private Vector3 GetViewportWorldPosition(float viewportX, float viewportY)
    {
        float distanceFromCamera = Mathf.Abs(transform.position.z - worldCamera.transform.position.z);

        Vector3 viewportPos = new Vector3(viewportX, viewportY, distanceFromCamera);
        Vector3 worldPos = worldCamera.ViewportToWorldPoint(viewportPos);

        worldPos.z = transform.position.z;
        return worldPos;
    }

    private Vector3 GetCurrentUIWorldTarget()
    {
        Camera currentUICamera = GetUICamera();
        Vector2 screenPos = manager.GetTargetUIScreenPosition(categoryId, currentUICamera);

        float distanceFromCamera = Mathf.Abs(transform.position.z - worldCamera.transform.position.z);

        Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceFromCamera));
        worldPos.z = transform.position.z;
        return worldPos;
    }

    private Camera GetUICamera()
    {
        if (uiCanvas == null)
            return null;

        if (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return uiCanvas.worldCamera;
    }
}