using UnityEditor;
using UnityEngine;

public class ConvertMaterialsToURP
{
    [MenuItem("Tools/Convert All Materials to URP Lit")]
    public static void ConvertAllMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");

        int convertedCount = 0;
        int skippedCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null)
            {
                Debug.LogWarning($"⚠️ Material null di path: {path}");
                continue;
            }

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("❌ Shader URP/Lit tidak ditemukan! Pastikan URP terinstal.");
                return;
            }

            // Ganti shader jika belum URP
            if (mat.shader != urpLit)
            {
                mat.shader = urpLit;
                EditorUtility.SetDirty(mat);
                convertedCount++;
            }
            else
            {
                skippedCount++;
            }

            // Coba pindahkan texture lama ke _BaseMap
            if (mat.HasProperty("_BaseMap") && mat.HasProperty("_MainTex"))
            {
                Texture mainTex = mat.GetTexture("_MainTex");
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                }
            }

            // Set fallback warna jika tidak ada texture
            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == null)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", Color.gray); // biar tidak putih terang
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Selesai konversi: {convertedCount} material berhasil diubah ke URP/Lit, {skippedCount} sudah sesuai.");
    }
}
