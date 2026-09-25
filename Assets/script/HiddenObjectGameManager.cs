using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class HiddenObjectGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalTargetObjects = 10;
    [SerializeField] private int maxLives = 3;

    [Header("Hint UI & Camera (NEW)")]
    [SerializeField] private CameraDrag2D cameraController;
    [SerializeField] private HintMarkerUI hintMarkerUI;
    [SerializeField] private float focusHintMarkerDuration = 1.5f;

    [Header("Power Up Settings")]
    [Tooltip("Drag all your hidden object GameObjects from the scene into this list!")]
    public List<GameObject> allHiddenObjects; 
    
    [Tooltip("The UI element at the bottom where the magnet will drag the item")]
    [SerializeField] private RectTransform bottomInventoryUI;

    [Header("UI")]
    [SerializeField] private Text collectedText;
    [SerializeField] private Text livesText;

    [Header("Managers")]
    [SerializeField] private LevelCompleteManagerBengkel levelCompleteManager; 
    [SerializeField] private GameOverManagerBandar gameOverManager;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip defeatSfx;
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip wrongSfx;

    private int collectedCount = 0;
    private int currentLives;
    private bool levelCompleted = false;
    private bool gameOver = false;

    private void Start()
    {
        currentLives = maxLives;
        UpdateUI();
    }

    public void HandleCorrectClick()
    {
        if (levelCompleted || gameOver) return;

        collectedCount++;
        PlaySfx(correctSfx);
        UpdateUI();

        if (collectedCount >= totalTargetObjects)
        {
            levelCompleted = true;
            PlaySfx(victorySfx);

            if (levelCompleteManager != null)
                levelCompleteManager.ShowCompletePanel();
        }
    }

    public void HandleWrongClick()
    {
        if (levelCompleted || gameOver) return;

        currentLives--;
        PlaySfx(wrongSfx);
        UpdateUI();

        if (currentLives <= 0)
        {
            currentLives = 0;
            gameOver = true;
            UpdateUI();
            PlaySfx(defeatSfx);

            if (gameOverManager != null)
                gameOverManager.ShowGameOverPanel();
        }
    }

    public bool UseFocusHint()
    {
        GameObject target = GetValidTargetItem();
        if (target == null) return false;

        if (cameraController != null)
            cameraController.FocusOnWorldPosition(target.transform.position);
        else
            Camera.main.transform.DOMove(new Vector3(target.transform.position.x, target.transform.position.y, Camera.main.transform.position.z), 0.5f);

        if (hintMarkerUI != null)
            hintMarkerUI.ShowOnTarget(target.transform, focusHintMarkerDuration);
        else
            target.transform.DOPunchScale(Vector3.one * 0.5f, 1f, 5, 1f); // Fallback bounce
        
        return true;
    }

    public bool UseMagnetHint()
    {
        CleanUpItemList();
        if (allHiddenObjects.Count == 0) return false;

        GameObject targetItem = null;
        bool isOnScreen = false;

        foreach (GameObject item in allHiddenObjects)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(item.transform.position);
            if (vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1 && vp.z > 0)
            {
                targetItem = item;
                isOnScreen = true;
                break;
            }
        }

        if (targetItem == null) targetItem = allHiddenObjects[0];
        allHiddenObjects.Remove(targetItem);

        Sequence magnetSequence = DOTween.Sequence();

        if (!isOnScreen)
        {
            if (cameraController != null)
            {
                cameraController.FocusOnWorldPosition(targetItem.transform.position);
                magnetSequence.AppendInterval(0.8f); // Wait for the pan to finish
            }
            else
            {
                Vector3 camTarget = new Vector3(targetItem.transform.position.x, targetItem.transform.position.y, Camera.main.transform.position.z);
                magnetSequence.Append(Camera.main.transform.DOMove(camTarget, 1.0f).SetEase(Ease.OutCubic));
            }
        }

        if (bottomInventoryUI != null)
        {
            Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(bottomInventoryUI.position);
            targetWorldPos.z = targetItem.transform.position.z;

            magnetSequence.Append(targetItem.transform.DOMove(targetWorldPos, 1.2f).SetEase(Ease.InBack));
            magnetSequence.Join(targetItem.transform.DORotate(new Vector3(0, 0, 360), 1.2f, RotateMode.FastBeyond360));
        }
        else
        {
            magnetSequence.Append(targetItem.transform.DOScale(0f, 0.5f));
        }

        magnetSequence.OnComplete(() =>
        {
            HandleCorrectClick(); 
            targetItem.SetActive(false); 
        });

        return true;
    }

    private GameObject GetValidTargetItem()
    {
        CleanUpItemList();
        if (allHiddenObjects.Count == 0) return null;
        return allHiddenObjects[Random.Range(0, allHiddenObjects.Count)];
    }

    private void CleanUpItemList()
    {
        allHiddenObjects.RemoveAll(item => item == null || !item.activeInHierarchy);
    }

    private void UpdateUI()
    {
        if (collectedText != null) collectedText.text = "Jumpa: " + collectedCount + "/" + totalTargetObjects;
        if (livesText != null) livesText.text = "Nyawa: " + currentLives;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
            sfxAudioSource.PlayOneShot(clip);
    }

    public int GetCollectedCount() { return collectedCount; }
    public int GetCurrentLives() { return currentLives; }
    public bool IsLevelCompleted() { return levelCompleted; }
    public bool IsGameOver() { return gameOver; }
}