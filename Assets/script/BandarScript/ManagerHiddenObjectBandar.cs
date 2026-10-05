using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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
    [SerializeField] private HiddenObjectCollapsibleUI collapsibleUI;

    [Header("Tick UI Animation")]
    [SerializeField] private TickAnimationTarget[] tickAnimationTargets;

    [Header("Focus Hint")]
    [SerializeField] private float focusHintMarkerDuration = 1.5f;

    [Header("Magnet: Risk/Reward Settings")]
    [SerializeField] private int magnetPullMaxCount = 3; 
    [SerializeField] private float magnetDelayBetweenObjects = 0.08f;
    [Tooltip("The logical distance. Make sure your ring image roughly matches this size visually!")]
    [SerializeField] private float magnetRadius = 3f; 
    
    [Header("Magnet: UI References")]
    [SerializeField] private GameObject popup1Instruction;
    [SerializeField] private GameObject popup2Confirm;
    [SerializeField] private GameObject ringPreviewGraphic; 

    private readonly List<HiddenObjectBandar> allObjects = new List<HiddenObjectBandar>();
    private readonly HashSet<HiddenObjectBandar> foundObjects = new HashSet<HiddenObjectBandar>();

    private Camera mainCamera;
    private bool levelCompleteTriggered = false;

    private bool isTargetingMode = false;
    private bool isConfirmingLocation = false;
    private Vector3 selectedWorldTargetPos;
    private PowerUpLimitBandar currentPowerUpUI;

    private void Awake()
    {
        RefreshReferences();
    }

    private IEnumerator Start()
    {
        yield return null;

        RefreshReferences();

        if (uiManager != null) uiManager.RebuildLookup();

        AutoRegisterSceneObjects();

        if (popup1Instruction != null) popup1Instruction.SetActive(false);
        if (popup2Confirm != null) popup2Confirm.SetActive(false);
        if (ringPreviewGraphic != null) ringPreviewGraphic.SetActive(false);
    }

    private void Update()
    {
        if (isTargetingMode && !isConfirmingLocation)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

                Vector3 mousePos = Input.mousePosition;
                selectedWorldTargetPos = mainCamera.ScreenToWorldPoint(mousePos);
                selectedWorldTargetPos.z = 0; 

                if (ringPreviewGraphic != null)
                {
                    ringPreviewGraphic.transform.position = selectedWorldTargetPos;
                    ringPreviewGraphic.SetActive(true);
                }

                isConfirmingLocation = true;
                if (popup2Confirm != null) popup2Confirm.SetActive(true);
            }
        }
    }

    public void RefreshReferences()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (uiManager == null) uiManager = FindObjectOfType<HiddenObjectUIManager>(true);
        if (cameraController == null) cameraController = FindObjectOfType<CameraDrag2D>(true);
        if (hintMarkerUI == null) hintMarkerUI = FindObjectOfType<HintMarkerUI>(true);
        if (levelCompleteManager == null) levelCompleteManager = FindObjectOfType<LevelCompleteManager>(true);
        if (wrongClickDetector == null) wrongClickDetector = FindObjectOfType<WrongClickDetectorBandar>(true);
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

            if (obj.IsFound) foundObjects.Add(obj);
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

        if (wrongClickDetector != null) wrongClickDetector.RegisterValidClick();

        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId))
        {
            uiManager.ConsumeOne(hiddenObject.CategoryId);
            PlayTickAnimation(hiddenObject.CategoryId);
            if (collapsibleUI != null) collapsibleUI.RegisterFoundItem(hiddenObject.CategoryId);
        }

        if (foundObjects.Count >= allObjects.Count)
        {
            if (!levelCompleteTriggered)
            {
                levelCompleteTriggered = true;
                TriggerLevelComplete();
            }
        }
        return true;
    }

    public bool TryMarkFound(HiddenObjectBandar hiddenObject, bool moveToUI)
    {
        bool accepted = TryMarkFound(hiddenObject);
        if (accepted && moveToUI) CompleteToUI(hiddenObject);
        return accepted;
    }

    public bool TryMarkFound(string targetId) { return TryMarkFound(FindObjectByCategory(targetId)); }
    public bool TryMarkFound(string targetId, bool moveToUI) { return TryMarkFound(FindObjectByCategory(targetId), moveToUI); }
    public bool TryMarkFound(string targetId, int instanceId) { return TryMarkFound(FindObjectByCategoryAndInstance(targetId, instanceId)); }
    public bool TryMarkFound(HiddenObjectBandar hiddenObject, int ignoredInstanceId) { return TryMarkFound(hiddenObject); }

    private void PlayTickAnimation(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId)) return;
        if (tickAnimationTargets == null || tickAnimationTargets.Length == 0) return;

        for (int i = 0; i < tickAnimationTargets.Length; i++)
        {
            TickAnimationTarget target = tickAnimationTargets[i];
            if (target == null || target.categoryId != categoryId) continue;
            if (target.tickAnimation != null) target.tickAnimation.Play();
            return;
        }
    }

    public object CenterTargetUI(HiddenObjectBandar hiddenObject)
    {
        if (hiddenObject == null) return null;
        RefreshReferences();
        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId)) StartCoroutine(uiManager.CenterObjectUISmooth(hiddenObject.CategoryId));
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
        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId)) return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, GetUICamera());
        if (mainCamera == null) return hiddenObject.transform.position;
        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(string targetId) { return GetTargetUIScreenPosition(FindObjectByCategory(targetId)); }

    public Vector3 GetTargetUIScreenPosition(string targetId, Camera uiCamera)
    {
        RefreshReferences();
        if (uiManager != null && !string.IsNullOrEmpty(targetId)) return uiManager.GetObjectUIScreenPosition(targetId, uiCamera);
        HiddenObjectBandar target = FindObjectByCategory(targetId);
        if (target == null) return Vector3.zero;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return target.transform.position;
        return mainCamera.WorldToScreenPoint(target.transform.position);
    }

    public Vector3 GetTargetUIScreenPosition(HiddenObjectBandar hiddenObject, Camera uiCamera)
    {
        if (hiddenObject == null) return Vector3.zero;
        if (uiManager != null && !string.IsNullOrEmpty(hiddenObject.CategoryId)) return uiManager.GetObjectUIScreenPosition(hiddenObject.CategoryId, uiCamera);
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return hiddenObject.transform.position;
        return mainCamera.WorldToScreenPoint(hiddenObject.transform.position);
    }

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

        if (cameraController != null) cameraController.FocusOnWorldPosition(target.transform.position);
        
        if (hintMarkerUI != null) hintMarkerUI.ShowOnTarget(target.transform, focusHintMarkerDuration);
        else target.PlayHint();

        if (uiManager != null && !string.IsNullOrEmpty(target.CategoryId))
            StartCoroutine(uiManager.CenterObjectUISmooth(target.CategoryId));

        return true;
    }

    public void StartMagnetTargeting(PowerUpLimitBandar uiController)
    {
        currentPowerUpUI = uiController;

        if (popup1Instruction != null) popup1Instruction.SetActive(true);
        if (popup2Confirm != null) popup2Confirm.SetActive(false);
        if (ringPreviewGraphic != null) ringPreviewGraphic.SetActive(false);
    }

    public void OnPopup1OKClicked()
    {
        if (popup1Instruction != null) popup1Instruction.SetActive(false);
        
        isTargetingMode = true;
        isConfirmingLocation = false;

        if (wrongClickDetector != null) wrongClickDetector.IsPaused = true;
    }

    public void OnPopup2TidakClicked()
    {
        isConfirmingLocation = false; 
        if (popup2Confirm != null) popup2Confirm.SetActive(false);
        if (ringPreviewGraphic != null) ringPreviewGraphic.SetActive(false);
    }

    public void OnPopup2YaClicked()
    {
        isTargetingMode = false;
        isConfirmingLocation = false;

        if (popup2Confirm != null) popup2Confirm.SetActive(false);
        if (ringPreviewGraphic != null) ringPreviewGraphic.SetActive(false);

        if (wrongClickDetector != null) wrongClickDetector.IsPaused = false;

        if (currentPowerUpUI != null) currentPowerUpUI.DeductMagnetCurrency();

        ExecuteRadiusMagnet();
    }

    private void ExecuteRadiusMagnet()
    {
        List<HiddenObjectBandar> itemsInRadius = new List<HiddenObjectBandar>();

        foreach (var obj in allObjects)
        {
            if (obj == null || foundObjects.Contains(obj) || !obj.gameObject.activeInHierarchy || obj.IsFound) continue;

            float distance = Vector2.Distance(selectedWorldTargetPos, obj.transform.position);
            if (distance <= magnetRadius)
            {
                itemsInRadius.Add(obj);
            }
        }

        int count = Mathf.Min(magnetPullMaxCount, itemsInRadius.Count);
        HiddenObjectBandar[] targetsToPull = new HiddenObjectBandar[count];
        
        for (int i = 0; i < count; i++) 
        {
            targetsToPull[i] = itemsInRadius[i];
        }

        if (targetsToPull.Length > 0)
        {
            StartCoroutine(MagnetRoutine(targetsToPull));
        }
        else
        {
            Debug.Log("[Magnet] Gambled and lost! No items inside the ring.");
        }
    }

    private IEnumerator MagnetRoutine(HiddenObjectBandar[] targets)
    {
        int totalTargets = targets.Length;

        for (int i = 0; i < targets.Length; i++)
        {
            HiddenObjectBandar target = targets[i];
            if (target == null) continue;

            if (target.BeginMagnetSelection())
            {
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
        if (levelCompleteManager != null) levelCompleteManager.ShowLevelComplete();
    }
}