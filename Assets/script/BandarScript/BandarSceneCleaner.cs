using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BandarSceneCleaner : MonoBehaviour
{
    private void Awake()
    {
        RemoveExtraEventSystems();
        RemoveMainMenuUI();
    }

    private void RemoveExtraEventSystems()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true);

        bool keptOne = false;
        for (int i = 0; i < systems.Length; i++)
        {
            if (!keptOne)
            {
                keptOne = true;
                continue;
            }

            Debug.Log("Destroy extra EventSystem: " + systems[i].gameObject.name);
            Destroy(systems[i].gameObject);
        }
    }

    private void RemoveMainMenuUI()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);

        for (int i = 0; i < canvases.Length; i++)
        {
            string n = canvases[i].gameObject.name.ToLower();

            if (n.Contains("mainmenu") || n.Contains("levelselect") || n.Contains("menu"))
            {
                Debug.Log("Destroy stray menu canvas: " + canvases[i].gameObject.name);
                Destroy(canvases[i].gameObject);
            }
        }

        MainMenuController[] menus = FindObjectsOfType<MainMenuController>(true);
        for (int i = 0; i < menus.Length; i++)
        {
            Debug.Log("Destroy stray MainMenuController: " + menus[i].gameObject.name);
            Destroy(menus[i].gameObject);
        }
    }
}