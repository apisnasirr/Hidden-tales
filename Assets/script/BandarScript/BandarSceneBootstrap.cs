using UnityEngine;

public class BandarSceneBootstrap : MonoBehaviour
{
    private void Awake()
    {
        CleanupPersistentObjects();
    }

    private void CleanupPersistentObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in allObjects)
        {
            if (obj == null)
                continue;

            // Jangan delete loading system
            if (obj.GetComponent<LoadingScreenManager>() != null)
                continue;

            if (obj.name == "LoadingScreenManager")
                continue;

            if (obj.name == "LoadingCanvas")
                continue;

            if (obj.name == "LoadingPanel")
                continue;

            // Jangan delete audio/currency/global manager
            if (obj.GetComponent<BGMManager>() != null)
                continue;

            if (obj.GetComponent<SFXManager>() != null)
                continue;

            if (obj.GetComponent<CurrencyManager>() != null)
                continue;
        }
    }
}