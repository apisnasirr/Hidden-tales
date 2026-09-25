using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ManagerHiddenObjectBandar : MonoBehaviour
{
    [System.Serializable]
    public class TickAnimationTarget
    {
        public string categoryId;
        public TickPopupAnimation tickAnimation;
    }

    [Header("References")]
    [SerializeField] private HiddenObjectUIManager uiManager;
    [SerializeField] private CameraDrag2D cameraController;
    [SerializeField] private HintMarkerUI hintMarkerUI;
    [SerializeField] private LevelCompleteManager levelCompleteManager;
    [SerializeField] private WrongClickDetectorBandar wrongClickDetector;

    [Header("Tick UI Animation")]
    [SerializeField] private TickAnimationTarget[] tickAnimationTargets;

    [Header("Focus Hint")]
    [SerializeField] private float focusHintMarkerDuration = 1.5f;

    [Header("Magnet")]
    [SerializeField] private int magnetPullCount = 2; // Can be set to 1 in inspector if you only want it to pull one item
    [SerializeField] private float magnetDelayBetweenObjects = 0.08f;

    private readonly List<HiddenObjectBandar> allObjects = new List<HiddenObjectBandar>();
    private readonly HashSet<HiddenObjectBandar> foundObjects = new HashSet<HiddenObjectBandar>();

    private Camera mainCamera;
    private bool levelCompleteTriggered = false;
    private Coroutine cameraHintRoutine;
    private Coroutine zoomPulseRoutine;

    private void Awake()
    {
        RefreshReferences();
    }

    private IEnumerator Start()
    {
        yield return null;

        RefreshReferences();

        if (uiManager != null)
            uiManager.RebuildLookup();

        AutoRegisterSceneObjects();
    }

    public void RefreshReferences()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (uiManager == null)
            uiManager = FindObjectOfType<HiddenObjectUIManager>(true);

        if (cameraController == null)
            cameraController = FindObjectOfType<CameraDrag2D>(true);

        if (hintMarkerUI == null)
            hintMarkerUI = FindObjectOfType<HintMarkerUI>(true);

        if (levelCompleteManager == null)
            levelCompleteManager = FindObjectOfType<LevelCompleteManager>(true);

        if (wrongClickDetector == null)
            wrongClickDetector = FindObjectOfType<WrongClickDetectorBandar>(true);
    }

    private void AutoRegisterSceneObjects()
    {
        allObjects.Clear();
        foundObjects.Clear();
        levelCompleteTriggered = false;

        Scene activeScene = SceneManager.GetActiveScene();
        HiddenObjectBandar[] objs = FindObjectsOfType<HiddenObjectBandar>(true);

        for (int i = 0; i < objs.Length; i++)
        {
            HiddenObjectBandar obj = objs[i];

            if (obj == null) continue;
            if (obj.gameObject.scene != activeScene) continue;

            RegisterObject(obj);

            if (obj.IsFound)
                foundObjects.Add(obj);
        }
    }

    public void RegisterObject(HiddenObjectBandar hiddenObject)
    {
        if (hiddenObject == null) return;
        if (allObjects.Contains(hiddenObject)) return;

        allObjects.Add(hiddenObject);
    }

    public void UnregisterObject(HiddenObjectBandar hiddenObject)
    {
        if (hiddenObject == null) return;

        allObjects.Remove(hiddenObject);
        foundObjects.Remove(hiddenObject);
    }

    public bool TryMarkFound(HiddenObjectBandar hiddenObject)
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

        if (foundObjects.Count >= allObjects.Count)
        {
            Debug.Log("[ManagerBandar] SUCCESS: All " + allObjects.Count + " items in the scene have been found!");
            
            if (!levelCompleteTriggered)
            {
                levelCompleteTriggered = true;
                TriggerLevelComplete();
            }
        }
        else
        {
            Debug.Log("[ManagerBandar] Item found! Total found: " + foundObjects.Count + " / " + allObjects.Count);
        }

        return true;
    }

    public bool TryMarkFound(HiddenObjectBandar hiddenObject, bool moveToUI)
    {
        bool accepted = TryMarkFound(hiddenObject);

        if (accepted && moveToUI)
            CompleteToUI(hiddenObject);

        return accepted;
    }

    public bool TryMarkFound(string targetId)
    {
        HiddenObjectBandar target = FindObjectByCategory(targetId);
        return TryMarkFound(target);
    }

    public bool TryMarkFound(string targetId, bool moveToUI)
    {
        HiddenObjectBandar target = FindObjectByCategory(targetId);
        return TryMarkFound(target, moveToUI);
    }

    public bool TryMarkFound(string targetId, int instanceId)
    {
        HiddenObjectBandar target = FindObjectByCategoryAndInstance(targetId, instanceId);
        return TryMarkFound(target);
    }

    public bool TryMarkFound(HiddenObjectBandar hiddenObject, int ignoredInstanceId)
    {
        return TryMarkFound(hiddenObject);
    }

    private void PlayTickAnimation(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId)) return;

        if (tickAnimationTargets == null || tickAnimationTargets.Length == 0) return;

        for (int i = 0; i < tickAnimationTargets.Length; i++)
        {
            TickAnimationTarget target = tickAnimationTargets[i];

            if (target == null || target.categoryId != categoryId) continue;

            if (target.tickAnimation != null)
                target.tickAnimation.Play();

            return;
        }
    }

    public object CenterTargetUI(HiddenObjectBandar hiddenObject)
    {
        if (hiddenObject == null) return null;
        RefreshReferences();

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            StartCoroutine(uiManager.CenterObjectUISmooth(hiddenObject.CategoryId));

        return null;
    }

    public object CenterTargetUI(string targetId)
    {
        HiddenObjectBandar target = FindObjectByCategory(targetId);
        if (target == null) return null;
        return CenterTargetUI(target);
    }

    public void CompleteToUI(HiddenObjectBandar hiddenObject) { }

    public void CompleteToUI(string targetId) { }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectBandar hiddenObject)
    {
        if (hiddenObject == null) return Vector3.zero;
        RefreshReferences();

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, GetUICamera());

        if (mainCamera == null) return hiddenObject.transform.position;
        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(string targetId)
    {
        HiddenObjectBandar target = FindObjectByCategory(targetId);
        return GetTargetUIScreenPosition(target);
    }

    public Vector3 GetTargetUIScreenPosition(string targetId, Camera uiCamera)
    {
        RefreshReferences();

        if (uiManager != null && !string.IsNullOrEmpty(targetId))
            return uiManager.GetObjectUIScreenPosition(targetId, uiCamera);

        HiddenObjectBandar target = FindObjectByCategory(targetId);

        if (target == null) return Vector3.zero;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return target.transform.position;

        return mainCamera.WorldToScreenPoint(target.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectBandar hiddenObject, Camera uiCamera)
    {
        if (hiddenObject == null) return Vector3.zero;

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
            return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, uiCamera);

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return hiddenObject.transform.position;

        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    // --- FOCUS HINT ---
    public bool UseFocusHint()
    {
        RefreshReferences();
        HiddenObjectBandar target = GetFirstUnfoundObject();

        if (target == null)
        {
            AutoRegisterSceneObjects();
            target = GetFirstUnfoundObject();
        }

        if (target == null) return false;

        // FIXED: Uses your Camera Controller so it doesn't snap!
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

    // --- MAGNET HINT ---
    public bool UseMagnetHint()
    {
        RefreshReferences();

        // Get the target(s) based on your original logic
        HiddenObjectBandar[] targets = GetUnfoundObjects(magnetPullCount);

        if (targets.Length == 0)
        {
            AutoRegisterSceneObjects();
            targets = GetUnfoundObjects(magnetPullCount);
        }

        if (targets.Length == 0) return false;

        // Pan to the first target before pulling
        if (cameraController != null && targets[0] != null)
        {
            cameraController.FocusOnWorldPosition(targets[0].transform.position);
        }

        StartCoroutine(MagnetRoutine(targets));
        return true;
    }

    private IEnumerator MagnetRoutine(HiddenObjectBandar[] targets)
    {
        int totalTargets = targets.Length;

        // Wait a tiny bit for the camera to pan before the item flies
        yield return new WaitForSeconds(0.4f); 

        for (int i = 0; i < targets.Length; i++)
        {
            HiddenObjectBandar target = targets[i];

            if (target == null) continue;

            if (target.BeginMagnetSelection())
            {
                // Uses the animation logic built into your item script!
                yield return target.PlayMagnetMoveToCenter(i, totalTargets);
                yield return target.PlayMagnetMoveToUI();
            }

            yield return new WaitForSeconds(magnetDelayBetweenObjects);
        }
    }

    private HiddenObjectBandar GetFirstUnfoundObject()
    {
        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectBandar obj = allObjects[i];
            if (obj == null || foundObjects.Contains(obj) || !obj.gameObject.activeInHierarchy || obj.IsFound) continue;
            return obj;
        }
        return null;
    }

    private HiddenObjectBandar[] GetUnfoundObjects(int count)
    {
        List<HiddenObjectBandar> result = new List<HiddenObjectBandar>();
        for (int i = 0; i < allObjects.Count; i++)
        {
            HiddenObjectBandar obj = allObjects[i];
            if (obj == null || foundObjects.Contains(obj) || !obj.gameObject.activeInHierarchy || obj.IsFound) continue;
            
            result.Add(obj);
            if (result.Count >= count) break;
        }
        return result.ToArray();
    }

    private HiddenObjectBandar FindObjectByCategory(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId)) return null;
        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] != null && allObjects[i].CategoryId == categoryId)
                return allObjects[i];
        }
        return null;
    }

    private HiddenObjectBandar FindObjectByCategoryAndInstance(string categoryId, int instanceId)
    {
        if (string.IsNullOrEmpty(categoryId)) return null;
        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] != null && allObjects[i].CategoryId == categoryId && allObjects[i].GetInstanceID() == instanceId)
                return allObjects[i];
        }
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
        if (levelCompleteManager != null)
        {
            Debug.Log("[ManagerBandar] Calling ShowLevelComplete() on the LevelCompleteManager now!");
            levelCompleteManager.ShowLevelComplete();
        }
        else
        {
            Debug.LogError("[ManagerBandar] FATAL ERROR: LevelCompleteManager slot is empty! Cannot show Win Panel.");
        }
    }
}