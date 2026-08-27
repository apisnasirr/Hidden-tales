using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HiddenObjectUIManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjectUIEntry
    {
        public string categoryId;
        public RectTransform uiRoot;
        public TMP_Text countText;
        public GameObject tickObject;
        public int totalCount = 1;

        [HideInInspector] public int remainingCount;
    }

    [Header("UI Entries")]
    [SerializeField] private List<ObjectUIEntry> objectUIList = new List<ObjectUIEntry>();

    [Header("Scroll")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private float scrollDuration = 0.25f;

    private readonly Dictionary<string, ObjectUIEntry> uiLookup = new Dictionary<string, ObjectUIEntry>();

    private void Awake()
    {
        RebuildLookup();
    }

    public void RebuildLookup()
    {
        if (scrollRect != null)
        {
            if (viewport == null) viewport = scrollRect.viewport;
            if (content == null) content = scrollRect.content;
        }

        uiLookup.Clear();

        foreach (var entry in objectUIList)
        {
            if (string.IsNullOrEmpty(entry.categoryId) || entry.uiRoot == null)
                continue;

            if (uiLookup.ContainsKey(entry.categoryId))
                continue;

            entry.remainingCount = Mathf.Max(0, entry.totalCount);

            if (entry.countText != null)
            {
                entry.countText.gameObject.SetActive(true);
                entry.countText.text = entry.remainingCount.ToString();
            }

            if (entry.tickObject != null)
                entry.tickObject.SetActive(false);

            uiLookup.Add(entry.categoryId, entry);
        }
    }

    public RectTransform GetObjectUIRect(string categoryId)
    {
        if (uiLookup.TryGetValue(categoryId, out ObjectUIEntry entry))
            return entry.uiRoot;

        return null;
    }

    public IEnumerator CenterObjectUISmooth(string categoryId)
    {
        if (scrollRect == null || viewport == null || content == null)
            yield break;

        RectTransform targetRect = GetObjectUIRect(categoryId);
        if (targetRect == null)
            yield break;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        scrollRect.StopMovement();
        scrollRect.velocity = Vector2.zero;

        Vector3 itemCenterWorld = targetRect.TransformPoint(targetRect.rect.center);
        Vector3 itemCenterInViewport = viewport.InverseTransformPoint(itemCenterWorld);

        float viewportCenterX = viewport.rect.center.x;
        float offsetX = viewportCenterX - itemCenterInViewport.x;

        Vector2 startPos = content.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(offsetX, 0f);

        float hiddenWidth = Mathf.Max(0f, content.rect.width - viewport.rect.width);
        float minX = -hiddenWidth;
        float maxX = 0f;

        endPos.x = Mathf.Clamp(endPos.x, minX, maxX);

        float time = 0f;
        while (time < scrollDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / scrollDuration);

            content.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        content.anchoredPosition = endPos;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public Vector2 GetObjectUIScreenPosition(string categoryId, Camera uiCamera = null)
    {
        RectTransform targetRect = GetObjectUIRect(categoryId);
        if (targetRect == null)
            return Vector2.zero;

        Vector3 worldCenter = targetRect.TransformPoint(targetRect.rect.center);
        return RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter);
    }

    public void ConsumeOne(string categoryId)
    {
        if (!uiLookup.TryGetValue(categoryId, out ObjectUIEntry entry))
            return;

        entry.remainingCount = Mathf.Max(0, entry.remainingCount - 1);

        if (entry.remainingCount > 0)
        {
            if (entry.countText != null)
            {
                entry.countText.gameObject.SetActive(true);
                entry.countText.text = entry.remainingCount.ToString();
            }

            if (entry.tickObject != null)
                entry.tickObject.SetActive(false);
        }
        else
        {
            if (entry.countText != null)
                entry.countText.gameObject.SetActive(false);

            if (entry.tickObject != null)
                entry.tickObject.SetActive(true);
        }

        Canvas.ForceUpdateCanvases();
    }

    public bool AreAllObjectsCollected()
    {
        if (uiLookup.Count == 0)
            return false;

        bool allCollected = true;

        foreach (var pair in uiLookup)
        {
            if (pair.Value.remainingCount > 0)
            {
                // This will print the EXACT item and how many are missing!
                Debug.LogWarning("[UIManager] Masih tunggu " + pair.Value.remainingCount + " item untuk kategori: " + pair.Key);
                allCollected = false;
            }
        }

        return allCollected;
    }

    public int GetTotalRemainingCount()
    {
        int total = 0;

        foreach (var pair in uiLookup)
            total += Mathf.Max(0, pair.Value.remainingCount);

        return total;
    }

    public int GetTotalTargetCount()
    {
        int total = 0;

        foreach (var entry in objectUIList)
            total += Mathf.Max(0, entry.totalCount);

        return total;
    }
}