using UnityEngine;
using UnityEngine.EventSystems;

public class WrongClickDetectorBandar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LifeManagerBandar lifeManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private WrongClickMarkUI wrongClickMarkUI;

    [Header("Click Settings")]
    [SerializeField] private float dragThreshold = 20f;

    private bool validClickThisFrame = false;
    private Vector2 mouseDownPosition;
    private bool isPointerDown = false;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void RegisterValidClick()
    {
        validClickThisFrame = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPointerDown = true;
            mouseDownPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!isPointerDown)
                return;

            isPointerDown = false;

            Vector2 mouseUpPosition = Input.mousePosition;

            if (Vector2.Distance(mouseDownPosition, mouseUpPosition) > dragThreshold)
            {
                validClickThisFrame = false;
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                validClickThisFrame = false;
                return;
            }

            if (validClickThisFrame)
            {
                validClickThisFrame = false;
                return;
            }

            CheckClick(mouseUpPosition);
            validClickThisFrame = false;
        }
    }

    private void CheckClick(Vector2 mouseScreenPos)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        Vector2 point2D = new Vector2(worldPos.x, worldPos.y);

        Collider2D hit2D = Physics2D.OverlapPoint(point2D);

        if (hit2D != null)
        {
            if (IsHiddenCharacter(hit2D.gameObject))
                return;

            if (hit2D.CompareTag("CorrectObject"))
                return;

            DeductLife(mouseScreenPos);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
        if (Physics.Raycast(ray, out RaycastHit hit3D))
        {
            if (IsHiddenCharacter(hit3D.collider.gameObject))
                return;

            if (hit3D.collider.CompareTag("CorrectObject"))
                return;

            DeductLife(mouseScreenPos);
            return;
        }

        DeductLife(mouseScreenPos);
    }

    private bool IsHiddenCharacter(GameObject obj)
    {
        if (obj.CompareTag("HiddenCharacter"))
            return true;

        if (obj.GetComponent<HiddenCharacterSequence>() != null)
            return true;

        if (obj.GetComponent<HiddenCharacterReward>() != null)
            return true;

        if (obj.GetComponent<HiddenCharacterController>() != null)
            return true;

        return false;
    }

    private void DeductLife(Vector2 clickScreenPos)
    {
        if (lifeManager != null)
            lifeManager.LoseLife();

        if (wrongClickMarkUI != null)
            wrongClickMarkUI.ShowAtScreenPosition(clickScreenPos);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayWrongClick();
    }
}