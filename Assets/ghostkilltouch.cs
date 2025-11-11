using UnityEngine;

public class GhostKillOnTouch : MonoBehaviour
{
    [Tooltip("Script yang mengatur efek jatuh kamera.")]
    public DeathCameraFall deathCam;

    bool hasAttacked = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasAttacked) return;

        // Cek apakah yang nabrak punya XR Origin atau tag Player
        if (other.CompareTag("Player"))
        {
            hasAttacked = true;
            if (deathCam != null)
            {
                deathCam.KillPlayer(); // atau deathCam.KillPlayer()
            }
        }
    }
}
