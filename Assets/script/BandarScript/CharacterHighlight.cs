using System; // <-- NEW: Required for Events
using UnityEngine;

public class CharacterHighlight : MonoBehaviour
{
    // --- NEW: This event tells the rest of the game that the tutorial character was clicked! ---
    public static event Action OnTutorialCharacterCollected;

    [Header("Visuals")]
    [SerializeField] private GameObject highlightGroup;
    [SerializeField] private Transform bobbingArrow;

    [Header("Animation")]
    [SerializeField] private float bobSpeed = 5f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startArrowPos;
    private Collider col3D;
    private Collider2D col2D;
    
    private bool isTutorialActive = false; // Remembers if THIS is the first character

    private void Awake()
    {
        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();

        if (highlightGroup != null)
            highlightGroup.SetActive(false); 

        if (bobbingArrow != null)
            startArrowPos = bobbingArrow.localPosition;
    }

    private void Update()
    {
        if (highlightGroup == null || !highlightGroup.activeSelf)
            return;

        bool isCollected = false;
        
        if (col3D != null && !col3D.enabled) isCollected = true;
        if (col2D != null && !col2D.enabled) isCollected = true;

        if (isCollected)
        {
            // --- NEW: If this was the tutorial character, trigger the shop animation! ---
            if (isTutorialActive)
            {
                OnTutorialCharacterCollected?.Invoke();
                isTutorialActive = false; // Stop it from firing twice
            }

            HideHighlight();
            return;
        }

        // Bobbing math
        if (bobbingArrow != null)
        {
            float newY = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            bobbingArrow.localPosition = startArrowPos + new Vector3(0f, newY, 0f);
        }
    }

    public void ShowHighlight()
    {
        if (highlightGroup != null)
            highlightGroup.SetActive(true);
            
        isTutorialActive = true; // Mark this character as the tutorial target
    }

    public void HideHighlight()
    {
        if (highlightGroup != null)
            highlightGroup.SetActive(false);
            
        isTutorialActive = false;
    }
}