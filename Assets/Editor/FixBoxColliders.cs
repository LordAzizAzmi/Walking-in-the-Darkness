using UnityEngine;
using UnityEditor;

public class FixBoxColliders : MonoBehaviour
{
    [MenuItem("Tools/Fix BoxCollider Scale & Size")]
    static void FixAllBoxColliders()
    {
        int fixedCount = 0;
        foreach (BoxCollider box in FindObjectsOfType<BoxCollider>())
        {
            Transform t = box.transform;

            // Cek jika ada skala negatif
            Vector3 scale = t.localScale;
            if (scale.x < 0 || scale.y < 0 || scale.z < 0)
            {
                scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                t.localScale = scale;
                fixedCount++;
            }

            // Pastikan size juga positif
            Vector3 size = box.size;
            box.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        }

        Debug.Log($"[FixBoxColliders] Fixed {fixedCount} BoxColliders with negative scale.");
    }
}
