using System.Collections;
using UnityEngine;

public class HintMarkerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer markerRenderer;

    [Header("Style")]
    [SerializeField] private Color markerColor = Color.yellow;
    [SerializeField] private Vector3 markerScale = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0f, 0f);

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 8f;        // How fast it bubbles
    [SerializeField] private float pulseIntensity = 0.2f;  // How much bigger/smaller it gets

    [Header("Debug")]
    [SerializeField] private bool debugShowOnStart = false;
    [SerializeField] private Vector3 debugWorldPosition = new Vector3(0f, 0f, 0f);

    private Coroutine showRoutine;
    private Transform currentTarget;

    private void Awake()
    {
        if (markerRenderer == null)
            markerRenderer = GetComponent<SpriteRenderer>();

        if (markerRenderer != null)
        {
            markerRenderer.color = markerColor;
            transform.localScale = markerScale;
            markerRenderer.enabled = false;
        }

        Debug.Log("[HintMarkerUI WORLD] Awake | renderer: " + (markerRenderer != null));
    }

    private void Start()
    {
        if (debugShowOnStart)
            DebugShowAtWorldPosition();
    }

    private void Update()
    {
        if (currentTarget == null || markerRenderer == null || !markerRenderer.enabled)
            return;

        transform.position = currentTarget.position + worldOffset;
    }

    public void ShowOnTarget(Transform target, float duration)
    {
        if (target == null)
        {
            Debug.LogWarning("[HintMarkerUI WORLD] target null");
            return;
        }

        if (markerRenderer == null)
        {
            Debug.LogWarning("[HintMarkerUI WORLD] markerRenderer null");
            return;
        }

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        currentTarget = target;
        showRoutine = StartCoroutine(ShowRoutine(duration));

        Debug.Log("[HintMarkerUI WORLD] ShowOnTarget -> " + target.name + " | duration: " + duration);
    }

    public void DebugShowAtWorldPosition()
    {
        if (markerRenderer == null)
            return;

        currentTarget = null;
        transform.position = debugWorldPosition + worldOffset;
        transform.localScale = markerScale;
        markerRenderer.color = markerColor;
        markerRenderer.enabled = true;

        Debug.Log("[HintMarkerUI WORLD] Debug marker shown at world position: " + transform.position);
    }

    private IEnumerator ShowRoutine(float duration)
    {
        markerRenderer.color = markerColor;
        transform.position = currentTarget.position + worldOffset;
        markerRenderer.enabled = true;

        float time = 0f;

        while (time < duration && currentTarget != null)
        {
            time += Time.deltaTime;
            
            // Track the target's position
            transform.position = currentTarget.position + worldOffset;
            
            // --- NEW: Bubbly Animation Math ---
            // Mathf.Sin goes smoothly back and forth between -1 and 1.
            float scaleModifier = 1f + (Mathf.Sin(time * pulseSpeed) * pulseIntensity);
            transform.localScale = markerScale * scaleModifier;

            yield return null;
        }

        // Reset everything when the timer runs out
        transform.localScale = markerScale; 
        markerRenderer.enabled = false;
        currentTarget = null;
        showRoutine = null;

        Debug.Log("[HintMarkerUI WORLD] Marker hidden");
    }
}