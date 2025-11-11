using UnityEngine;

public class GhostAttackTrigger : MonoBehaviour
{
    public float attackRange = 1.2f;
    public Transform player;          // drag: Main Camera (head)
    public DeathCameraFall deathCam;  // drag: DeathCameraFall di Main Camera

    bool hasAttacked = false;

    void Reset()
    {
        if (player == null && Camera.main) player = Camera.main.transform;
        if (deathCam == null && Camera.main) deathCam = Camera.main.GetComponent<DeathCameraFall>();
    }

    void Update()
    {
        if (hasAttacked || player == null || deathCam == null) return;

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            hasAttacked = true;
            deathCam.KillPlayer();   // atau deathCam.KillPlayer();
        }
    }

    // Alternatif kalau pakai trigger collider:
    void OnTriggerEnter(Collider other)
    {
        if (hasAttacked || deathCam == null) return;
        if (other.transform == player)
        {
            hasAttacked = true;
            deathCam.KillPlayer();
        }
    }
}
