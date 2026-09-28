using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using DG.Tweening;
using System.Collections.Generic;

public class HiddenObjectCollapsibleUI : MonoBehaviour
{
    [Header("Main Container")]
    [Tooltip("The parent holding everything (usually BottomHolder)")]
    [SerializeField] private RectTransform bottomHolder;

    [Header("Backgrounds & Content")]
    [Tooltip("The large white board image CanvasGroup")]
    [SerializeField] private CanvasGroup expandedBackground;
    [Tooltip("The thin blue bar image CanvasGroup")]
    [SerializeField] private CanvasGroup minimizedBackground;
    [Tooltip("The CanvasGroup holding the Scroll View and all items")]
    [SerializeField] private CanvasGroup itemContent;
    [Tooltip("The CanvasGroup holding the text counters")]
    [SerializeField] private CanvasGroup counterContent;

    [Header("Buttons")]
    [SerializeField] private Button minimizeArrowButton;
    [SerializeField] private Button expandArrowButton;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [Tooltip("How far down the BottomHolder slides when minimized")]
    [SerializeField] private float minimizedYPosition = -200f; 

    [Header("Counters")]
    [SerializeField] private TMP_Text bendaHidupText;
    [SerializeField] private TMP_Text bendaBukanHidupText;

    [Header("Item Categories")]
    public List<string> bendaHidupIDs;
    public List<string> bendaBukanHidupIDs;

    private int foundHidup = 0;
    private int foundBukanHidup = 0;
    private float expandedYPosition;

    private void Start()
    {
        if (bottomHolder != null) expandedYPosition = bottomHolder.anchoredPosition.y;

        if (minimizeArrowButton != null) minimizeArrowButton.onClick.AddListener(ShowMinimized);
        if (expandArrowButton != null) expandArrowButton.onClick.AddListener(ShowExpanded);

        UpdateCounterText();
        ShowExpanded(); 
    }

    public void RegisterFoundItem(string categoryId)
    {
        if (bendaHidupIDs.Contains(categoryId)) foundHidup++;
        else if (bendaBukanHidupIDs.Contains(categoryId)) foundBukanHidup++;

        UpdateCounterText();
    }

    private void UpdateCounterText()
    {
        if (bendaHidupText != null) bendaHidupText.text = $"{foundHidup}/{bendaHidupIDs.Count}";
        if (bendaBukanHidupText != null) bendaBukanHidupText.text = $"{foundBukanHidup}/{bendaBukanHidupIDs.Count}";
    }

    private void ShowMinimized()
    {
        minimizeArrowButton.gameObject.SetActive(false);
        expandArrowButton.gameObject.SetActive(true);

        bottomHolder.DOAnchorPosY(minimizedYPosition, slideDuration).SetEase(Ease.InOutBack);

        expandedBackground.DOFade(0f, slideDuration * 0.8f);
        itemContent.DOFade(0f, slideDuration * 0.8f).OnComplete(() => itemContent.blocksRaycasts = false);
        
        minimizedBackground.DOFade(1f, slideDuration);
        counterContent.DOFade(1f, slideDuration);
    }

    private void ShowExpanded()
    {
        expandArrowButton.gameObject.SetActive(false);
        minimizeArrowButton.gameObject.SetActive(true);

        bottomHolder.DOAnchorPosY(expandedYPosition, slideDuration).SetEase(Ease.InOutBack);

        minimizedBackground.DOFade(0f, slideDuration * 0.8f);
        counterContent.DOFade(0f, slideDuration * 0.8f);

        expandedBackground.DOFade(1f, slideDuration);
        itemContent.DOFade(1f, slideDuration).OnStart(() => itemContent.blocksRaycasts = true);
    }
}