using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCharacterInstructionTrigger : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Characters")]
    [Tooltip("Masukkan 3 character dekat sini. Boleh guna untuk scene Bandar, Runcit, Bengkel.")]
    [SerializeField] private Transform[] characters;

    [Header("Optional Auto Find By Tag")]
    [SerializeField] private bool autoFindCharactersByTag = false;
    [SerializeField] private string characterTag = "HiddenCharacter";

    [Header("Instruction Popup")]
    [SerializeField] private GameObject instructionPopup;
    [SerializeField] private CanvasGroup instructionCanvasGroup;
    [SerializeField] private Button closeButton;

    [Header("Popup Setting")]
    [Tooltip("Kalau true, popup hanya muncul sekali sahaja untuk seluruh scene walaupun ada 3 character.")]
    [SerializeField] private bool showOnlyOnceForWholeScene = false;

    [Tooltip("Kalau false, setiap character boleh trigger popup sekali. Sesuai kalau ada 3 character.")]
    [SerializeField] private bool showOncePerCharacter = true;

    [Header("Detection Setting")]
    [SerializeField] private bool detectOnlyWhenPopupClosed = true;
    [SerializeField] private float checkInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private readonly HashSet<Transform> triggeredCharacters = new HashSet<Transform>();

    private bool hasShownOnceForWholeScene = false;
    private bool isPopupOpen = false;
    private float checkTimer = 0f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (instructionPopup != null && instructionCanvasGroup == null)
            instructionCanvasGroup = instructionPopup.GetComponent<CanvasGroup>();

        if (instructionPopup != null && instructionCanvasGroup == null)
            instructionCanvasGroup = instructionPopup.AddComponent<CanvasGroup>();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseInstructionPopup);
            closeButton.onClick.AddListener(CloseInstructionPopup);
        }
    }

    private void Start()
    {
        if (autoFindCharactersByTag)
            FindCharactersByTag();

        HideInstructionPopupInstant();

        if (debugMode)
        {
            Debug.Log("[CameraCharacterInstructionTrigger] Script aktif pada: " + gameObject.name);
            Debug.Log("[CameraCharacterInstructionTrigger] Jumlah character: " + characters.Length);

            if (targetCamera == null)
                Debug.LogError("[CameraCharacterInstructionTrigger] Target Camera belum assign.");

            if (instructionPopup == null)
                Debug.LogError("[CameraCharacterInstructionTrigger] Instruction Popup belum assign.");

            if (closeButton == null)
                Debug.LogWarning("[CameraCharacterInstructionTrigger] Close Button belum assign.");
        }
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        if (instructionPopup == null)
            return;

        if (detectOnlyWhenPopupClosed && isPopupOpen)
            return;

        if (showOnlyOnceForWholeScene && hasShownOnceForWholeScene)
            return;

        checkTimer += Time.deltaTime;

        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        CheckCharactersInCamera();
    }

    private void FindCharactersByTag()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(characterTag);

        characters = new Transform[foundObjects.Length];

        for (int i = 0; i < foundObjects.Length; i++)
            characters[i] = foundObjects[i].transform;
    }

    private void CheckCharactersInCamera()
    {
        if (characters == null || characters.Length == 0)
            return;

        for (int i = 0; i < characters.Length; i++)
        {
            Transform character = characters[i];

            if (character == null)
                continue;

            if (showOncePerCharacter && triggeredCharacters.Contains(character))
                continue;

            if (IsCharacterVisibleByCamera(character))
            {
                TriggerInstruction(character);
                return;
            }
        }
    }

    private bool IsCharacterVisibleByCamera(Transform character)
    {
        Renderer characterRenderer = character.GetComponentInChildren<Renderer>();

        if (characterRenderer != null)
        {
            Bounds bounds = characterRenderer.bounds;

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            Vector3[] points =
            {
                center,
                center + new Vector3(extents.x, extents.y, extents.z),
                center + new Vector3(extents.x, extents.y, -extents.z),
                center + new Vector3(extents.x, -extents.y, extents.z),
                center + new Vector3(extents.x, -extents.y, -extents.z),
                center + new Vector3(-extents.x, extents.y, extents.z),
                center + new Vector3(-extents.x, extents.y, -extents.z),
                center + new Vector3(-extents.x, -extents.y, extents.z),
                center + new Vector3(-extents.x, -extents.y, -extents.z)
            };

            for (int i = 0; i < points.Length; i++)
            {
                if (IsWorldPointInsideCamera(points[i]))
                    return true;
            }

            return false;
        }

        return IsWorldPointInsideCamera(character.position);
    }

    private bool IsWorldPointInsideCamera(Vector3 worldPosition)
    {
        Vector3 viewportPoint = targetCamera.WorldToViewportPoint(worldPosition);

        bool inFrontOfCamera = viewportPoint.z > 0f;
        bool insideHorizontal = viewportPoint.x >= 0f && viewportPoint.x <= 1f;
        bool insideVertical = viewportPoint.y >= 0f && viewportPoint.y <= 1f;

        return inFrontOfCamera && insideHorizontal && insideVertical;
    }

    private void TriggerInstruction(Transform character)
    {
        if (character == null)
            return;

        if (showOncePerCharacter)
            triggeredCharacters.Add(character);

        if (showOnlyOnceForWholeScene)
            hasShownOnceForWholeScene = true;

        if (debugMode)
            Debug.Log("[CameraCharacterInstructionTrigger] Camera detect character: " + character.name);

        ShowInstructionPopup();
    }

    private void ShowInstructionPopup()
    {
        isPopupOpen = true;

        instructionPopup.SetActive(true);
        instructionPopup.transform.SetAsLastSibling();

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 1f;
            instructionCanvasGroup.interactable = true;
            instructionCanvasGroup.blocksRaycasts = true;
        }
    }

    public void CloseInstructionPopup()
    {
        HideInstructionPopupInstant();
    }

    private void HideInstructionPopupInstant()
    {
        isPopupOpen = false;

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 0f;
            instructionCanvasGroup.interactable = false;
            instructionCanvasGroup.blocksRaycasts = false;
        }

        if (instructionPopup != null)
            instructionPopup.SetActive(false);
    }

    public void ResetInstructionTrigger()
    {
        triggeredCharacters.Clear();
        hasShownOnceForWholeScene = false;
        isPopupOpen = false;

        HideInstructionPopupInstant();

        if (debugMode)
            Debug.Log("[CameraCharacterInstructionTrigger] Trigger instruction di-reset.");
    }
}