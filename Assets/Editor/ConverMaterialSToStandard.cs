using UnityEditor;
using UnityEngine;

public class FixWhiteMaterials
{
    [MenuItem("Tools/Fix White Materials")]
    public static void FixWhiteMats()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            bool changed = false;

            // Pastikan shader standard
            if (mat.shader.name != "Standard")
            {
                mat.shader = Shader.Find("Standard");
                changed = true;
            }

            // Perbaiki jika tidak ada _MainTex tapi punya _BaseMap (sisa dari URP)
            if (!mat.HasProperty("_MainTex")) continue;

            var mainTex = mat.GetTexture("_MainTex");
            var baseMap = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;

            if (mainTex == null && baseMap != null)
            {
                mat.SetTexture("_MainTex", baseMap);
                changed = true;
            }

            // Periksa warna putih polos
            if (mat.HasProperty("_Color"))
            {
                var color = mat.GetColor("_Color");
                if (color == Color.white || color.a < 1f)
                {
                    mat.SetColor("_Color", new Color(1f, 1f, 1f, 1f)); // bisa kamu ganti jadi keabu-abuan jika perlu
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(mat);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"🔧 Selesai! Material putih yang diperbaiki: {fixedCount}");
    }
}
