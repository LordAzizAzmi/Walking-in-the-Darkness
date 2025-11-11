using UnityEngine;

public class FollowHead : MonoBehaviour
{
    public Transform cameraTransform;
    public float followDistance = 1.5f;
    public float heightOffset = 0f;

    void Update()
    {
        // Ambil forward tanpa pitch
        Vector3 forwardFlat = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

        // Posisi UI berada di depan, rotasi horizontal (kiri kanan)
        Vector3 targetPosition = cameraTransform.position + forwardFlat * followDistance;
        targetPosition.y = cameraTransform.position.y + heightOffset;
        transform.position = targetPosition;

        // Rotasi UI agar selalu menghadap ke kamera, tapi tetap tegak (tidak miring ke atas/bawah)
        transform.LookAt(cameraTransform.position);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}
