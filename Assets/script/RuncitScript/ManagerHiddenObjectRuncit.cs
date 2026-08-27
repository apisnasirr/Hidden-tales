using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManagerHiddenObjectRuncit : MonoBehaviour
{
    [System.Serializable]
    public class TickAnimationTarget
    {
        public string categoryId;
        public TickPopupAnimation tickAnimation;
    }

    [Header("References")]
    [SerializeField] private HiddenObjectUIManagerRuncit uiManager;
    [SerializeField] private CameraDrag2DRuncit cameraController;
    [SerializeField] private HintMarkerUIRuncit hintMarkerUI;
    [SerializeField] private LevelCompleteManagerKedaiRuncit levelCompleteManager;
    [SerializeField] private WrongClickDetectorRuncit wrongClickDetector;

    [Header("Tick UI Animation")]
    [SerializeField] private TickAnimationTarget[] tickAnimationTargets;

    [Header("Scroll Lock")]
    [SerializeField] private ScrollRect itemListScrollRect;
    [SerializeField] private bool lockScrollWhenCollectItem = true;
    [SerializeField] private float scrollLockDuration = 0.75f;

    [Header("Focus Hint")]
    [SerializeField] private float focusHintMarkerDuration = 1.5f;

    [Header("Zoom Hint")]
    [SerializeField] private float zoomHintSize = 3.5f;
    [SerializeField] private float zoomHintHoldDuration = 1.8f;
    [SerializeField] private float zoomHintDelayBeforePulse = 0.35f;

    [Header("Zoom Hint Item Highlight")]
    [SerializeField] private float zoomHintPulseDuration = 1f;
    [SerializeField] private float zoomHintPulseSpeed = 5f;
    [SerializeField] private float zoomHintBiggerScale = 1.12f;
    [SerializeField] private float zoomHintSmallerScale = 0.95f;
    [SerializeField] private Color zoomHintColor = Color.yellow;

    [Header("Magnet")]
    [SerializeField] private int magnetPullCount = 2;
    [SerializeField] private float magnetDelayBetweenObjects = 0.08f;

    private readonly List<HiddenObjectRuncit> allObjects = new List<HiddenObjectRuncit>();
    private readonly HashSet<HiddenObjectRuncit> foundObjects = new HashSet<HiddenObjectRuncit>();

    private Camera mainCamera;
    private bool levelCompleteTriggered = false;
    private Coroutine cameraHintRoutine;
    private Coroutine zoomPulseRoutine;

    private Coroutine scrollLockRoutine;
    private Vector2 savedContentAnchoredPosition;
    private float savedHorizontalNormalizedPosition;
    private float savedVerticalNormalizedPosition;

    private bool isScrollFrozen = false;
    private bool savedScrollHorizontal = true;
    private bool savedScrollVertical = true;
    private bool savedScrollInertia = true;

    private void Awake()
    {
        RefreshReferences();
    }

    private void OnEnable()
    {
        Canvas.willRenderCanvases += RestoreFrozenScrollBeforeRender;
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= RestoreFrozenScrollBeforeRender;

        if (isScrollFrozen)
            RestoreScrollSettings();
    }

    private IEnumerator Start()
    {
        yield return null;

        RefreshReferences();
        AutoRegisterSceneObjects();
    }

    private void LateUpdate()
    {
        if (isScrollFrozen)
            RestoreScrollPosition();
    }

    public void RefreshReferences()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (uiManager == null)
            uiManager = FindObjectOfType<HiddenObjectUIManagerRuncit>(true);

        if (cameraController == null)
            cameraController = FindObjectOfType<CameraDrag2DRuncit>(true);

        if (hintMarkerUI == null)
            hintMarkerUI = FindObjectOfType<HintMarkerUIRuncit>(true);

        if (levelCompleteManager == null)
            levelCompleteManager = FindObjectOfType<LevelCompleteManagerKedaiRuncit>(true);

        if (wrongClickDetector == null)
            wrongClickDetector = FindObjectOfType<WrongClickDetectorRuncit>(true);
    }

    private void AutoRegisterSceneObjects()
    {
        allObjects.Clear();
        foundObjects.Clear();
        levelCompleteTriggered = false;

        Scene activeScene = SceneManager.GetActiveScene();
        HiddenObjectRuncit[] objs = FindObjectsOfType<HiddenObjectRuncit>(true);

        for (int i = 0; i < objs.Length; i++)
        {
            HiddenObjectRuncit obj = objs[i];

            if (obj == null) continue;
            if (obj.gameObject.scene != activeScene) continue;

            RegisterObject(obj);

            if (obj.IsFound)
                foundObjects.Add(obj);
        }
    }

    public void RegisterObject(HiddenObjectRuncit hiddenObject)
    {
        if (hiddenObject == null) return;
        if (allObjects.Contains(hiddenObject)) return;

        allObjects.Add(hiddenObject);
    }

    public void UnregisterObject(HiddenObjectRuncit hiddenObject)
    {
        if (hiddenObject == null) return;

        allObjects.Remove(hiddenObject);
        foundObjects.Remove(hiddenObject);
    }

    public bool TryMarkFound(HiddenObjectRuncit hiddenObject)
    {
        if (hiddenObject == null) return false;
        if (foundObjects.Contains(hiddenObject)) return false;

        foundObjects.Add(hiddenObject);

        if (wrongClickDetector != null)
            wrongClickDetector.RegisterValidClick();

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
        {
            uiManager.ConsumeOne(hiddenObject.CategoryId);
            PlayTickAnimation(hiddenObject.CategoryId);
        }

        // --- THE NEW WIN LOGIC ---
        if (foundObjects.Count >= allObjects.Count)
        {
            Debug.Log("[Manager] SUCCESS: All " + allObjects.Count + " items in the scene have been found!");
            
            if (!levelCompleteTriggered)
            {
                levelCompleteTriggered = true;
                TriggerLevelComplete();
            }
        }
        else
        {
            Debug.Log("[Manager] Item found! Total found: " + foundObjects.Count + " / " + allObjects.Count);
        }
        return true;
    }


    public bool TryMarkFound(HiddenObjectRuncit hiddenObject, bool moveToUI)
    {
        bool accepted = TryMarkFound(hiddenObject);

        if (accepted && moveToUI)
            CompleteToUI(hiddenObject);

        return accepted;
    }

    public bool TryMarkFound(string targetId)
    {
        HiddenObjectRuncit target = FindObjectByCategory(targetId);
        return TryMarkFound(target);
    }

    public bool TryMarkFound(string targetId, bool moveToUI)
    {
        HiddenObjectRuncit target = FindObjectByCategory(targetId);
        return TryMarkFound(target, moveToUI);
    }

    public bool TryMarkFound(string targetId, int instanceId)
    {
        HiddenObjectRuncit target = FindObjectByCategoryAndInstance(targetId, instanceId);
        return TryMarkFound(target);
    }

    public bool TryMarkFound(HiddenObjectRuncit hiddenObject, int ignoredInstanceId)
    {
        return TryMarkFound(hiddenObject);
    }

    private void PlayTickAnimation(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId))
            return;

        if (tickAnimationTargets == null || tickAnimationTargets.Length == 0)
            return;

        for (int i = 0; i < tickAnimationTargets.Length; i++)
        {
            TickAnimationTarget target = tickAnimationTargets[i];

            if (target == null)
                continue;

            if (target.categoryId != categoryId)
                continue;

            if (target.tickAnimation != null)
                target.tickAnimation.Play();

            return;
        }
    }

    private void CaptureScrollPosition()
    {
        if (!lockScrollWhenCollectItem)
            return;

        if (itemListScrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        savedHorizontalNormalizedPosition = itemListScrollRect.horizontalNormalizedPosition;
        savedVerticalNormalizedPosition = itemListScrollRect.verticalNormalizedPosition;

        if (itemListScrollRect.content != null)
            savedContentAnchoredPosition = itemListScrollRect.content.anchoredPosition;

        savedScrollHorizontal = itemListScrollRect.horizontal;
        savedScrollVertical = itemListScrollRect.vertical;
        savedScrollInertia = itemListScrollRect.inertia;

        itemListScrollRect.StopMovement();
        itemListScrollRect.velocity = Vector2.zero;

        itemListScrollRect.inertia = false;
        itemListScrollRect.horizontal = false;
        itemListScrollRect.vertical = false;

        isScrollFrozen = true;
    }

    private void StartScrollLock()
    {
        if (!lockScrollWhenCollectItem)
            return;

        if (itemListScrollRect == null)
            return;

        if (scrollLockRoutine != null)
            StopCoroutine(scrollLockRoutine);

        scrollLockRoutine = StartCoroutine(KeepScrollStillRoutine());
    }

    private IEnumerator KeepScrollStillRoutine()
    {
        yield return new WaitForEndOfFrame();

        float timer = 0f;

        while (timer < scrollLockDuration)
        {
            RestoreScrollPosition();

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreScrollPosition();
        RestoreScrollSettings();

        scrollLockRoutine = null;
    }

    private void RestoreFrozenScrollBeforeRender()
    {
        if (isScrollFrozen)
            RestoreScrollPosition();
    }

    private void RestoreScrollPosition()
    {
        if (itemListScrollRect == null)
            return;

        itemListScrollRect.StopMovement();
        itemListScrollRect.velocity = Vector2.zero;

        itemListScrollRect.horizontalNormalizedPosition = savedHorizontalNormalizedPosition;
        itemListScrollRect.verticalNormalizedPosition = savedVerticalNormalizedPosition;

        if (itemListScrollRect.content != null)
            itemListScrollRect.content.anchoredPosition = savedContentAnchoredPosition;
    }

    private void RestoreScrollSettings()
    {
        if (itemListScrollRect != null)
        {
            itemListScrollRect.horizontal = savedScrollHorizontal;
            itemListScrollRect.vertical = savedScrollVertical;
            itemListScrollRect.inertia = savedScrollInertia;

            itemListScrollRect.StopMovement();
            itemListScrollRect.velocity = Vector2.zero;
        }

        isScrollFrozen = false;
    }

    private bool AreAllHiddenObjectsFound()
    {
        if (allObjects.Count == 0)
            return false;

        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectRuncit obj = allObjects[i];

            if (obj == null)
                continue;

            if (foundObjects.Contains(obj))
                continue;

            if (obj.IsFound)
                continue;

            return false;
        }

        return true;
    }

    public object CenterTargetUI(HiddenObjectRuncit hiddenObject)
    {
        if (hiddenObject == null) return null;

        RefreshReferences();

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            StartCoroutine(uiManager.CenterObjectUISmooth(hiddenObject.CategoryId));

        return null;
    }

    public object CenterTargetUI(string targetId)
    {
        HiddenObjectRuncit target = FindObjectByCategory(targetId);
        if (target == null) return null;

        return CenterTargetUI(target);
    }

    public void CompleteToUI(HiddenObjectRuncit hiddenObject)
    {
    }

    public void CompleteToUI(string targetId)
    {
    }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectRuncit hiddenObject)
    {
        if (hiddenObject == null)
            return Vector3.zero;

        RefreshReferences();

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, GetUICamera());

        if (mainCamera == null)
            return hiddenObject.transform.position;

        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(string targetId)
    {
        HiddenObjectRuncit target = FindObjectByCategory(targetId);
        return GetTargetUIScreenPosition(target);
    }

    public Vector3 GetTargetUIScreenPosition(string targetId, Camera uiCamera)
    {
        RefreshReferences();

        if (uiManager != null && !string.IsNullOrEmpty(targetId))
            return uiManager.GetObjectUIScreenPosition(targetId, uiCamera);

        HiddenObjectRuncit target = FindObjectByCategory(targetId);

        if (target == null)
            return Vector3.zero;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return target.transform.position;

        return mainCamera.WorldToScreenPoint(target.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectRuncit hiddenObject, Camera uiCamera)
    {
        if (hiddenObject == null)
            return Vector3.zero;

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, uiCamera);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return hiddenObject.transform.position;

        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    public bool UseFocusHint()
    {
        RefreshReferences();

        HiddenObjectRuncit target = GetFirstUnfoundObject();

        if (target == null)
        {
            AutoRegisterSceneObjects();
            target = GetFirstUnfoundObject();
        }

        if (target == null)
        {
            Debug.LogWarning("UseFocusHint gagal: tiada hidden object yang belum dijumpai.");
            return false;
        }

        if (cameraController != null)
            cameraController.FocusOnWorldPosition(target.transform.position);
        else
            MoveCameraInstant(target.transform.position);

        if (hintMarkerUI != null)
            hintMarkerUI.ShowOnTarget(target.transform, focusHintMarkerDuration);
        else
            target.PlayHint();

        if (uiManager != null && !string.IsNullOrEmpty(target.CategoryId))
            StartCoroutine(uiManager.CenterObjectUISmooth(target.CategoryId));

        return true;
    }

    public bool UseZoomHint()
    {
        RefreshReferences();

        HiddenObjectRuncit target = GetFirstUnfoundObject();

        if (target == null)
        {
            AutoRegisterSceneObjects();
            target = GetFirstUnfoundObject();
        }

        if (target == null)
        {
            Debug.LogWarning("UseZoomHint gagal: tiada hidden object yang belum dijumpai.");
            return false;
        }

        StartCameraHintRoutine(ZoomHintRoutine(target));
        return true;
    }

    public bool UseMagnetPower()
    {
        HiddenObjectRuncit[] targets = GetUnfoundObjects(magnetPullCount);

        if (targets.Length == 0)
        {
            AutoRegisterSceneObjects();
            targets = GetUnfoundObjects(magnetPullCount);
        }

        if (targets.Length == 0)
        {
            Debug.LogWarning("UseMagnetPower gagal: tiada hidden object yang belum dijumpai.");
            return false;
        }

        StartCoroutine(MagnetRoutine(targets));
        return true;
    }

    private IEnumerator ZoomItemPulseRoutine(HiddenObjectRuncit target)
    {
        if (target == null)
            yield break;

        Transform targetTransform = target.transform;
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();

        Vector3 originalScale = targetTransform.localScale;
        Vector3 biggerScale = originalScale * zoomHintBiggerScale;
        Vector3 smallerScale = originalScale * zoomHintSmallerScale;

        Color originalColor = Color.white;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = zoomHintColor;
        }

        float timer = 0f;

        while (timer < zoomHintPulseDuration)
        {
            if (target == null)
                yield break;

            timer += Time.unscaledDeltaTime;

            float pulse = Mathf.PingPong(timer * zoomHintPulseSpeed, 1f);
            targetTransform.localScale = Vector3.Lerp(smallerScale, biggerScale, pulse);

            yield return null;
        }

        targetTransform.localScale = originalScale;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        zoomPulseRoutine = null;
    }

    private void StartCameraHintRoutine(IEnumerator routine)
    {
        if (cameraHintRoutine != null)
            StopCoroutine(cameraHintRoutine);

        cameraHintRoutine = StartCoroutine(routine);
    }

    private IEnumerator ZoomHintRoutine(HiddenObjectRuncit target)
    {
        if (target == null) yield break;

        RefreshReferences();

        if (cameraController != null)
        {
            cameraController.FocusOnWorldPosition(target.transform.position);
            cameraController.SetZoomTarget(zoomHintSize);
        }
        else
        {
            MoveCameraInstant(target.transform.position);
            SetCameraZoomInstant(zoomHintSize);
        }

        if (zoomHintDelayBeforePulse > 0f)
            yield return new WaitForSecondsRealtime(zoomHintDelayBeforePulse);

        if (zoomPulseRoutine != null)
            StopCoroutine(zoomPulseRoutine);

        zoomPulseRoutine = StartCoroutine(ZoomItemPulseRoutine(target));

        if (zoomHintHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(zoomHintHoldDuration);

        if (cameraController != null)
            cameraController.ResetZoom();

        cameraHintRoutine = null;
    }

    private void MoveCameraInstant(Vector3 targetPosition)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 camPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(targetPosition.x, targetPosition.y, camPos.z);
    }

    private void SetCameraZoomInstant(float size)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (mainCamera.orthographic)
            mainCamera.orthographicSize = size;
        else
            mainCamera.fieldOfView = size;
    }

    private IEnumerator MagnetRoutine(HiddenObjectRuncit[] targets)
    {
        int totalTargets = targets.Length;

        for (int i = 0; i < targets.Length; i++)
        {
            HiddenObjectRuncit target = targets[i];

            if (target == null)
                continue;

            if (target.BeginMagnetSelection())
            {
                yield return target.PlayMagnetMoveToCenter(i, totalTargets);
                yield return target.PlayMagnetMoveToUI();
            }

            yield return new WaitForSeconds(magnetDelayBetweenObjects);
        }
    }

    private HiddenObjectRuncit GetFirstUnfoundObject()
    {
        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectRuncit obj = allObjects[i];

            if (obj == null) continue;
            if (foundObjects.Contains(obj)) continue;
            if (!obj.gameObject.activeInHierarchy) continue;
            if (obj.IsFound) continue;

            return obj;
        }

        return null;
    }

    private HiddenObjectRuncit[] GetUnfoundObjects(int count)
    {
        List<HiddenObjectRuncit> result = new List<HiddenObjectRuncit>();

        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectRuncit obj = allObjects[i];

            if (obj == null) continue;
            if (foundObjects.Contains(obj)) continue;
            if (!obj.gameObject.activeInHierarchy) continue;
            if (obj.IsFound) continue;

            result.Add(obj);

            if (result.Count >= count)
                break;
        }

        return result.ToArray();
    }

    private HiddenObjectRuncit FindObjectByCategory(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId))
            return null;

        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectRuncit obj = allObjects[i];

            if (obj == null) continue;

            if (obj.CategoryId == categoryId)
                return obj;
        }

        return null;
    }

    private HiddenObjectRuncit FindObjectByCategoryAndInstance(string categoryId, int instanceId)
    {
        if (string.IsNullOrEmpty(categoryId))
            return null;

        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectRuncit obj = allObjects[i];

            if (obj == null) continue;
            if (obj.CategoryId != categoryId) continue;

            if (obj.GetInstanceID() == instanceId)
                return obj;
        }

        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectRuncit obj = allObjects[i];

            if (obj == null) continue;

            if (obj.CategoryId == categoryId)
                return obj;
        }

        return null;
    }

    private Camera GetUICamera()
    {
        if (uiManager == null)
            return null;

        Canvas canvas = uiManager.GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    private void TriggerLevelComplete()
    {
        if (levelCompleteManager != null)
            levelCompleteManager.ShowLevelComplete();
        else
            Debug.LogError("LevelCompleteManagerKedaiRuncit tidak dijumpai dalam scene.");
    }
}