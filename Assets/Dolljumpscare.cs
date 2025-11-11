using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class DollJumpscareAI : MonoBehaviour
{
    [Header("Target")]
    public Transform playerCamera; // XR Camera (Main Camera in XR Origin)

    [Header("Movement")]
    public float roamRadius = 15f;
    public float roamInterval = 5f;
    public float chaseDistance = 4f;
    public float jumpscareDistance = 2f;
    public float respawnDelay = 5f;

    [Header("Audio Settings")]
    public AudioClip roamingSound; // suara bisikan / langkah / statis halus
    public AudioClip jumpscareSound; // suara teriakan atau glitch keras
    public float roamVolume = 0.3f;
    public float jumpscareVolume = 1f;

    [Header("Jumpscare Settings")]
    public float jumpscareDuration = 1.5f;
    public float appearDistanceFromCamera = 0.8f;
    public float verticalOffset = 0.4f; // ✨ tambahan: biar boneka sejajar / melayang sedikit

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator; // ✨ otomatis deteksi animator kalau ada

    private bool isJumpscaring = false;
    private Vector3 spawnAreaCenter;
    private float roamTimer;
    private bool isPlayingRoamSound = false;
    private string currentAnim = "";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); // bisa null jika asset tanpa animasi

        // setup audio 3D
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;

        spawnAreaCenter = transform.position;
        roamTimer = roamInterval;

        // mainkan suara roaming secara looping
        if (roamingSound != null)
        {
            audioSource.clip = roamingSound;
            audioSource.loop = true;
            audioSource.volume = roamVolume;
            audioSource.Play();
            isPlayingRoamSound = true;
        }

        PlayAnim("Idle");
    }

    void Update()
    {
        if (isJumpscaring || playerCamera == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);

        if (distance <= jumpscareDistance)
        {
            StartCoroutine(DoJumpscare());
        }
        else if (distance <= chaseDistance)
        {
            // kejar player
            agent.SetDestination(playerCamera.position);
            PlayAnim("run");
        }
        else
        {
            // roam acak
            roamTimer += Time.deltaTime;
            if (roamTimer >= roamInterval)
            {
                Vector3 newPos = GetRandomNavmeshLocation(roamRadius);
                agent.SetDestination(newPos);
                roamTimer = 0f;
            }
            PlayAnim("run");
        }
    }

    IEnumerator DoJumpscare()
    {
        isJumpscaring = true;
        PlayAnim("jump"); // ✨ animasi loncat (kalau ada)

        // Matikan AI sementara
        agent.isStopped = true;
        agent.enabled = false;

        // hentikan suara roaming sementara
        if (isPlayingRoamSound)
        {
            audioSource.Stop();
            isPlayingRoamSound = false;
        }

        // === POSISI DEPAN KAMERA (melayang sejajar POV) ===
        Vector3 targetPos = playerCamera.position + playerCamera.forward * appearDistanceFromCamera;
        targetPos.y = playerCamera.position.y + verticalOffset;

        // === ANIMASI MELUNCUR KE DEPAN POV ===
        Vector3 startPos = transform.position;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.LookAt(playerCamera);
            yield return null;
        }

        // === HADAPKAN KE KAMERA SECARA PENUH ===
        transform.LookAt(playerCamera);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180f, 0);

        // === PERBESAR SEMENTARA (biar lebih intimidating) ===
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.3f;

        // === MAINKAN SUARA JUMPSCARE ===
        if (jumpscareSound)
        {
            audioSource.spatialBlend = 0.8f;
            audioSource.volume = jumpscareVolume;
            audioSource.PlayOneShot(jumpscareSound);
        }

        yield return new WaitForSeconds(jumpscareDuration);

        // === KEMBALIKAN SKALA DAN SEMBUNYIKAN ===
        transform.localScale = originalScale;
        gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);
        RespawnInRandomArea();

        // === HIDUPKAN KEMBALI SUARA ROAMING ===
        if (roamingSound != null)
        {
            audioSource.clip = roamingSound;
            audioSource.loop = true;
            audioSource.volume = roamVolume;
            audioSource.Play();
            isPlayingRoamSound = true;
        }

        PlayAnim("idle");
        isJumpscaring = false;
    }

    void RespawnInRandomArea()
    {
        Vector3 newPos = GetRandomNavmeshLocation(roamRadius);
        transform.position = newPos;
        gameObject.SetActive(true);
        agent.isStopped = false;
        agent.enabled = true;
    }

    Vector3 GetRandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += spawnAreaCenter;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            return hit.position;

        return spawnAreaCenter;
    }

    void PlayAnim(string animName)
    {
        if (animator == null) return; // ✨ tidak error kalau asset tanpa animasi
        if (currentAnim == animName) return;
        if (animator.HasState(0, Animator.StringToHash(animName)))
        {
            animator.CrossFade(animName, 0.1f);
            currentAnim = animName;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnAreaCenter == Vector3.zero ? transform.position : spawnAreaCenter, roamRadius);
    }
}
