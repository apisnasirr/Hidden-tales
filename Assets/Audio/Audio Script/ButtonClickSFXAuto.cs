using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonClickSFXAuto : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        AddSFXToAllButtons();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddSFXToAllButtons();
    }

    private void AddSFXToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            ButtonClickSFXMarker marker = button.GetComponent<ButtonClickSFXMarker>();

            if (marker == null)
                marker = button.gameObject.AddComponent<ButtonClickSFXMarker>();
        }

        Debug.Log("[ButtonClickSFXAuto] Semua button sudah dipasang click SFX. Total: " + buttons.Length);
    }
}