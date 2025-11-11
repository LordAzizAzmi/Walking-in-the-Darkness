using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AutoReassignTextures : MonoBehaviour
{
    [MenuItem("Tools/Reassign Lost Textures from Folder")]
    public static void ReassignTextures()
    {
        string textureFolderPath = "Assets/Flooded_Grounds/Materials"; // Ganti jika folder lain
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { textureFolderPath });

        Dictionary<string, Texture2D> textureMap = new Dictionary<string, Texture2D>();

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                string cleanName = Path.GetFileNameWithoutExtension(path).ToLower();
                if (!textureMap.ContainsKey(cleanName))
                    textureMap.Add(cleanName, tex);
            }
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int reassignCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            string matName = Path.GetFileNameWithoutExtension(path).ToLower();

            // Cari tekstur yang cocok dengan nama material
            foreach (var texEntry in textureMap)
            {
                if (matName.Contains(texEntry.Key) || texEntry.Key.Contains(matName))
                {
                    bool changed = false;

                    // Atur untuk Built-in Standard
                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", texEntry.Value);
                        changed = true;
                    }

                    // Atur juga untuk URP Lit
                    if (mat.HasProperty("_BaseMap"))
                    {
                        mat.SetTexture("_BaseMap", texEntry.Value);
                        changed = true;
                    }

                    if (changed)
                    {
                        EditorUtility.SetDirty(mat);
                        reassignCount++;
                        break;
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"🧩 Reassign selesai. Material yang berhasil diberi ulang tekstur: {reassignCount}");
    }
}
