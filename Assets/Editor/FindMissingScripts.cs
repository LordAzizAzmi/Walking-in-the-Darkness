using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts in Scene")]
    public static void FindMissingScriptsInScene()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        int goCount = 0, componentsCount = 0, missingCount = 0;
        foreach (GameObject g in go)
        {
            goCount++;
            Component[] components = g.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                componentsCount++;
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.Log("Missing script in: " + g.name, g);
                }
            }
        }

        Debug.Log($"Selesai! Diperiksa {goCount} GameObject, {componentsCount} komponen, ditemukan {missingCount} yang missing.");
    }
}
