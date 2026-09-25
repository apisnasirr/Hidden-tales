using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; // <-- REQUIRED FOR POWERUPS

public class ManagerHiddenObjectBengkel : MonoBehaviour
{
    [System.Serializable]
    public class TickAnimationTarget
    {
        public string categoryId;
        public TickPopupAnimation tickAnimation;
    }

    [Header("References")]
    [SerializeField] private HiddenObjectUIManagerBengkel uiManager;
    [SerializeField] private CameraDrag2DBengkel cameraController;
    [SerializeField] private HintMarkerUIBengkel hintMarkerUI;
    [SerializeField] private LevelCompleteManagerBengkel levelCompleteManager;
    [SerializeField] private WrongClickDetectorBengkel wrongClickDetector;

    [Header("Tick UI Animation")]
    [SerializeField] private TickAnimationTarget[] tickAnimationTargets;

    [Header("Scroll Lock")]
    [SerializeField] private ScrollRect itemListScrollRect;
    [SerializeField] private bool lockScrollWhenCollectItem = true;
    [SerializeField] private float scrollLockDuration = 0.75f;

    [Header("Focus Hint")]
    [SerializeField] private float focusHintMarkerDuration = 1.5f;

    [Header("Magnet Hint")]
    [SerializeField] private int magnetPullCount = 1; // Usually 1 for standard powerup
    [SerializeField] private float magnetDelayBetweenObjects = 0.08f;

    private readonly List<HiddenObjectBengkel> allObjects = new List<HiddenObjectBengkel>();
    private readonly HashSet<HiddenObjectBengkel> foundObjects = new HashSet<HiddenObjectBengkel>();

    private Camera mainCamera;
    private bool levelCompleteTriggered = false;

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
        if (isScrollFrozen) RestoreScrollSettings();
    }

    private IEnumerator Start()
    {
        yield return null;
        RefreshReferences();
        AutoRegisterSceneObjects();
    }

    private void LateUpdate()
    {
        if (isScrollFrozen) RestoreScrollPosition();
    }

    public void RefreshReferences()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (uiManager == null) uiManager = FindObjectOfType<HiddenObjectUIManagerBengkel>(true);
        if (cameraController == null) cameraController = FindObjectOfType<CameraDrag2DBengkel>(true);
        if (hintMarkerUI == null) hintMarkerUI = FindObjectOfType<HintMarkerUIBengkel>(true);
        if (levelCompleteManager == null) levelCompleteManager = FindObjectOfType<LevelCompleteManagerBengkel>(true);
        if (wrongClickDetector == null) wrongClickDetector = FindObjectOfType<WrongClickDetectorBengkel>(true);
    }

    private void AutoRegisterSceneObjects()
    {
        allObjects.Clear();
        foundObjects.Clear();
        levelCompleteTriggered = false;

        Scene activeScene = SceneManager.GetActiveScene();
        HiddenObjectBengkel[] objs = FindObjectsOfType<HiddenObjectBengkel>(true);

        for (int i = 0; i < objs.Length; i++)
        {
            HiddenObjectBengkel obj = objs[i];
            if (obj == null || obj.gameObject.scene != activeScene) continue;

            RegisterObject(obj);
            if (obj.IsFound) foundObjects.Add(obj);
        }
    }

    public void RegisterObject(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null || allObjects.Contains(hiddenObject)) return;
        allObjects.Add(hiddenObject);
    }

    public void UnregisterObject(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null) return;
        allObjects.Remove(hiddenObject);
        foundObjects.Remove(hiddenObject);
    }

    public bool TryMarkFound(HiddenObjectBengkel hiddenObject)
    {
        bool accepted = MarkFoundOnly(hiddenObject);
        if (accepted) FinishCollectUI(hiddenObject);
        return accepted;
    }

    public bool TryMarkFound(HiddenObjectBengkel hiddenObject, bool moveToUI)
    {
        bool accepted = MarkFoundOnly(hiddenObject);
        if (!accepted) return false;

        if (moveToUI)
            StartCoroutine(CompleteToUIRoutine(hiddenObject));
        else
            FinishCollectUI(hiddenObject);

        return true;
    }

    public bool TryMarkFound(string targetId) { return TryMarkFound(FindObjectByCategory(targetId), false); }
    public bool TryMarkFound(string targetId, bool moveToUI) { return TryMarkFound(FindObjectByCategory(targetId), moveToUI); }
    public bool TryMarkFound(string targetId, int instanceId) { return TryMarkFound(FindObjectByCategoryAndInstance(targetId, instanceId), false); }
    public bool TryMarkFound(HiddenObjectBengkel hiddenObject, int ignoredInstanceId) { return TryMarkFound(hiddenObject, false); }

    private bool MarkFoundOnly(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null || foundObjects.Contains(hiddenObject)) return false;

        foundObjects.Add(hiddenObject);
        if (wrongClickDetector != null) wrongClickDetector.RegisterValidClick();

        return true;
    }

    private IEnumerator CompleteToUIRoutine(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null) yield break;

        yield return hiddenObject.PlayMagnetMoveToCenter(0, 1);
        yield return hiddenObject.PlayMagnetMoveToUI();

        FinishCollectUI(hiddenObject);
    }

    private void FinishCollectUI(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null) return;

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
        {
            CaptureScrollPosition();
            uiManager.ConsumeOne(hiddenObject.CategoryId);
            PlayTickAnimation(hiddenObject.CategoryId);
            StartScrollLock();
        }

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
    }

    private void PlayTickAnimation(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId) || tickAnimationTargets == null || tickAnimationTargets.Length == 0) return;

        for (int i = 0; i < tickAnimationTargets.Length; i++)
        {
            TickAnimationTarget target = tickAnimationTargets[i];
            if (target != null && target.categoryId == categoryId && target.tickAnimation != null)
            {
                target.tickAnimation.Play();
                return;
            }
        }
    }

    private void CaptureScrollPosition()
    {
        if (!lockScrollWhenCollectItem || itemListScrollRect == null) return;

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
        if (!lockScrollWhenCollectItem || itemListScrollRect == null) return;

        if (scrollLockRoutine != null) StopCoroutine(scrollLockRoutine);
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

    private void RestoreFrozenScrollBeforeRender() { if (isScrollFrozen) RestoreScrollPosition(); }

    private void RestoreScrollPosition()
    {
        if (itemListScrollRect == null) return;

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

    public object CenterTargetUI(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null) return null;
        RefreshReferences();
        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            StartCoroutine(uiManager.CenterObjectUISmooth(hiddenObject.CategoryId));
        return null;
    }

    public object CenterTargetUI(string targetId) { return CenterTargetUI(FindObjectByCategory(targetId)); }
    public void CompleteToUI(HiddenObjectBengkel hiddenObject) { if (hiddenObject != null) StartCoroutine(CompleteToUIRoutine(hiddenObject)); }
    public void CompleteToUI(string targetId) { CompleteToUI(FindObjectByCategory(targetId)); }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectBengkel hiddenObject)
    {
        if (hiddenObject == null) return Vector3.zero;
        RefreshReferences();
        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, GetUICamera());

        if (mainCamera == null) return hiddenObject.transform.position;
        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(string targetId) { return GetTargetUIScreenPosition(FindObjectByCategory(targetId)); }

    public Vector3 GetTargetUIScreenPosition(string targetId, Camera uiCamera)
    {
        RefreshReferences();
        if (uiManager != null && !string.IsNullOrEmpty(targetId)) return uiManager.GetObjectUIScreenPosition(targetId, uiCamera);
        HiddenObjectBengkel target = FindObjectByCategory(targetId);
        if (target == null) return Vector3.zero;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return target.transform.position;
        return mainCamera.WorldToScreenPoint(target.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectBengkel hiddenObject, Camera uiCamera)
    {
        if (hiddenObject == null) return Vector3.zero;
        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId)) return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, uiCamera);
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return hiddenObject.transform.position;
        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    // --- FOCUS HINT (UPDATED FOR BENGKEL) ---
    public bool UseFocusHint()
    {
        RefreshReferences();
        HiddenObjectBengkel target = GetFirstUnfoundObject();

        if (target == null)
        {
            AutoRegisterSceneObjects();
            target = GetFirstUnfoundObject();
        }

        if (target == null) return false;

        if (cameraController != null)
            cameraController.FocusOnWorldPosition(target.transform.position);

        if (hintMarkerUI != null)
            hintMarkerUI.ShowOnTarget(target.transform, focusHintMarkerDuration);
        else
            target.PlayHint();

        if (uiManager != null && !string.IsNullOrEmpty(target.CategoryId))
            StartCoroutine(uiManager.CenterObjectUISmooth(target.CategoryId));

        return true;
    }

    // --- MAGNET HINT (UPDATED FOR BENGKEL) ---
    public bool UseMagnetHint()
    {
        RefreshReferences();
        HiddenObjectBengkel targetItem = GetFirstUnfoundObject();

        if (targetItem == null)
        {
            AutoRegisterSceneObjects();
            targetItem = GetFirstUnfoundObject();
        }

        if (targetItem == null) return false;

        // 1. Turn off colliders instantly
        Collider col3D = targetItem.GetComponent<Collider>();
        Collider2D col2D = targetItem.GetComponent<Collider2D>();
        if (col3D != null) col3D.enabled = false;
        if (col2D != null) col2D.enabled = false;

        // 2. Check if on screen
        Vector3 vp = mainCamera.WorldToViewportPoint(targetItem.transform.position);
        bool isOnScreen = vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1 && vp.z > 0;

        Sequence magnetSequence = DOTween.Sequence();

        // 3. Pan camera
        if (!isOnScreen)
        {
            if (cameraController != null)
            {
                cameraController.FocusOnWorldPosition(targetItem.transform.position);
                magnetSequence.AppendInterval(0.6f); 
            }
            else
            {
                Vector3 camTarget = new Vector3(targetItem.transform.position.x, targetItem.transform.position.y, mainCamera.transform.position.z);
                magnetSequence.Append(mainCamera.transform.DOMove(camTarget, 0.8f).SetEase(Ease.OutCubic));
            }
        }

        // 4. Find UI Slot
        bool hasUIPos = false;
        Vector3 targetWorldPos = Vector3.zero;

        if (uiManager != null && !string.IsNullOrEmpty(targetItem.CategoryId))
        {
            StartCoroutine(uiManager.CenterObjectUISmooth(targetItem.CategoryId));
            magnetSequence.AppendInterval(0.25f); 
            
            Vector2 screenPos = uiManager.GetObjectUIScreenPosition(targetItem.CategoryId, GetUICamera());
            if (screenPos != Vector2.zero)
            {
                targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                targetWorldPos.z = targetItem.transform.position.z;
                hasUIPos = true;
            }
        }

        // 5. Fly animation
        if (hasUIPos)
        {
            magnetSequence.Append(targetItem.transform.DOMove(targetWorldPos, 1.2f).SetEase(Ease.InBack));
            magnetSequence.Join(targetItem.transform.DORotate(new Vector3(0, 0, 360), 1.2f, RotateMode.FastBeyond360));
        }
        else
        {
            magnetSequence.Append(targetItem.transform.DOScale(0f, 0.5f));
        }

        // 6. Complete
        magnetSequence.OnComplete(() =>
        {
            targetItem.BeginMagnetSelection(); 
            targetItem.gameObject.SetActive(false); 
        });

        return true;
    }

    private HiddenObjectBengkel GetFirstUnfoundObject()
    {
        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectBengkel obj = allObjects[i];
            if (obj == null || foundObjects.Contains(obj) || !obj.gameObject.activeInHierarchy || obj.IsFound) continue;
            return obj;
        }
        return null;
    }

    private HiddenObjectBengkel FindObjectByCategory(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId)) return null;
        for (int i = 0; i < allObjects.Count; i++)
            if (allObjects[i] != null && allObjects[i].CategoryId == categoryId) return allObjects[i];
        return null;
    }

    private HiddenObjectBengkel FindObjectByCategoryAndInstance(string categoryId, int instanceId)
    {
        if (string.IsNullOrEmpty(categoryId)) return null;
        for (int i = 0; i < allObjects.Count; i++)
            if (allObjects[i] != null && allObjects[i].CategoryId == categoryId && allObjects[i].GetInstanceID() == instanceId) return allObjects[i];
        return FindObjectByCategory(categoryId);
    }

    private Camera GetUICamera()
    {
        if (uiManager == null) return null;
        Canvas canvas = uiManager.GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return canvas.worldCamera;
    }

    private void TriggerLevelComplete()
    {
        if (levelCompleteManager != null) levelCompleteManager.ShowLevelComplete();
    }
}