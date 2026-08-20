using UnityEngine;

public class HiddenObjectItem : MonoBehaviour
{
    [SerializeField] private HiddenObjectGameManager gameManager;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip objectSfx;

    private bool isFound = false;

    private void OnMouseDown()
    {
        if (isFound)
            return;

        isFound = true;

        if (gameManager != null)
            gameManager.HandleCorrectClick();

        if (sfxAudioSource != null && objectSfx != null)
            sfxAudioSource.PlayOneShot(objectSfx);

        gameObject.SetActive(false);
    }
}