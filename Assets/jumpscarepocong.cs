using UnityEngine;
using System.Collections;

public class PocongJumpscareSpawner : MonoBehaviour
{
    public GameObject pocongPrefab;           // Pocong object in the scene (not a prefab to instantiate)
    public Transform playerCamera;            // Camera XR or player view
    public float showDuration = 3f;           // How long Pocong is visible
    public float minDelay = 60f;              // 1 minute
    public float maxDelay = 300f;             // 3 minutes
    public float distanceInFront = 2.5f;      // Distance in front of player

    [Header("Jumpscare Audio")]
    public AudioClip jumpscareSound;          // Suara jumpscare
    private AudioSource audioSource;

    private void Start()
    {
        if (pocongPrefab != null)
            pocongPrefab.SetActive(false);

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f; // 🔥 bikin 2D, jadi langsung ke telinga player
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;

        StartCoroutine(JumpscareLoop());
    }


    IEnumerator JumpscareLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            ShowPocong();
            yield return new WaitForSeconds(showDuration);
            HidePocong();
        }
    }

    void ShowPocong()
    {
        if (playerCamera == null || pocongPrefab == null) return;

        // Tentukan arah ke depan kamera
        Vector3 forward = playerCamera.forward;
        forward.y = 0;
        forward.Normalize();

        // Tentukan posisi awal spawn (di depan player, tapi masih di udara)
        Vector3 spawnOrigin = playerCamera.position + forward * distanceInFront;
        spawnOrigin.y += 2f; // mulai dari atas kepala agar pasti kena tanah

        RaycastHit hit;
        Vector3 spawnPos;

        // Raycast dari atas ke bawah untuk cari posisi tanah
        if (Physics.Raycast(spawnOrigin, Vector3.down, out hit, 10f))
        {
            spawnPos = hit.point;
        }
        else
        {
            // Jika tidak kena tanah, fallback ke posisi default
            spawnPos = playerCamera.position + forward * distanceInFront;
            spawnPos.y = playerCamera.position.y;
        }

        // Set posisi Pocong
        pocongPrefab.transform.position = spawnPos;

        // Hadapkan Pocong ke player
        Vector3 lookAt = new Vector3(playerCamera.position.x, spawnPos.y, playerCamera.position.z);
        pocongPrefab.transform.LookAt(lookAt);

        // Kunci rotasi X -90 agar tetap berdiri
        Vector3 euler = pocongPrefab.transform.eulerAngles;
        euler.x = -90f;
        pocongPrefab.transform.eulerAngles = euler;

        // Tampilkan Pocong
        pocongPrefab.SetActive(true);

        // Mainkan suara jumpscare
        if (jumpscareSound != null && audioSource != null)
            audioSource.PlayOneShot(jumpscareSound);
    }

    void HidePocong()
    {
        if (pocongPrefab != null)
            pocongPrefab.SetActive(false);
    }
}

