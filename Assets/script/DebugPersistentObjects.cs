using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugPersistentObjects : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("===== DEBUG OBJECTS IN SCENE: " + SceneManager.GetActiveScene().name + " =====");

        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject go = allObjects[i];
            if (go == null) continue;

            Debug.Log("Object: " + go.name + " | Scene: " + go.scene.name);
        }
    }
}