using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelSceneBootstrap : MonoBehaviour
{
    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        yield return null;

        FixHiddenCanvasGroups();
        EnsureMainCameraTag();
        EnsureSingleEventSystem();
        RefreshLevelReferences();
    }

    private void FixHiddenCanvasGroups()
    {
        CanvasGroup[] canvasGroups = FindObjectsOfType<CanvasGroup>(true);

        for (int i = 0; i < canvasGroups.Length; i++)
        {
            CanvasGroup cg = canvasGroups[i];
            if (cg == null) continue;

            bool visuallyHidden = !cg.gameObject.activeInHierarchy || cg.alpha <= 0.001f;

            if (visuallyHidden)
            {
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }

    private void EnsureMainCameraTag()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null)
            return;

        Camera[] cameras = FindObjectsOfType<Camera>(true);

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;

            cameras[i].tag = "MainCamera";
            Debug.Log("LevelSceneBootstrap: tag MainCamera dipasang pada " + cameras[i].name);
            return;
        }
    }

    private void EnsureSingleEventSystem()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true);

        if (systems.Length <= 1)
            return;

        for (int i = 1; i < systems.Length; i++)
        {
            if (systems[i] != null)
                Destroy(systems[i].gameObject);
        }

        Debug.Log("LevelSceneBootstrap: EventSystem duplicate dibuang.");
    }

    private void RefreshLevelReferences()
    {
        ManagerHiddenObjectBandar managerBandar = FindObjectOfType<ManagerHiddenObjectBandar>(true);
        if (managerBandar != null)
            managerBandar.RefreshReferences();

        WrongClickDetectorBandar wrongClickBandar = FindObjectOfType<WrongClickDetectorBandar>(true);
        if (wrongClickBandar != null)
            SendRefreshMessage(wrongClickBandar);

        HiddenObjectBandar[] hiddenObjects = FindObjectsOfType<HiddenObjectBandar>(true);
        for (int i = 0; i < hiddenObjects.Length; i++)
        {
            if (hiddenObjects[i] != null)
                hiddenObjects[i].RefreshAfterSceneLoad();
        }
    }

    private void SendRefreshMessage(MonoBehaviour target)
    {
        if (target == null) return;

        var method = target.GetType().GetMethod("RefreshReferences",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        if (method != null)
            method.Invoke(target, null);
    }
}