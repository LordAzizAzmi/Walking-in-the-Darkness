using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class GhostRoamAutoAI : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;                   // Player XR / Kamera utama
    public float activationDistance = 2f;      // Jarak untuk mengaktifkan hantu

    [Header("Roaming Area")]
    public float roamRadius = 15f;
    public float roamInterval = 5f;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotateSpeed = 5f;

    [Header("Audio Settings")]
    public AudioClip roamSound;
    public float roamVolume = 0.5f;

    [Header("Animation Settings (Auto)")]
    public Animator animator;
    public bool useAnimation = true;           // true = gunakan animasi jika tersedia
    private string detectedAnimState = "";     // otomatis scan state di Animator

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Vector3 spawnCenter;
    private float roamTimer;
    private bool isActive = false;
    private bool hasAnimator = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        spawnCenter = transform.position;
        roamTimer = roamInterval;

        // cek apakah ada animator
        animator = GetComponent<Animator>();
        hasAnimator = (animator != null);

        // scan animasi jika ada
        if (hasAnimator)
        {
            detectedAnimState = DetectWalkAnimation();
            if (!string.IsNullOrEmpty(detectedAnimState))
                Debug.Log($"[GhostRoamAutoAI] Detected animation: {detectedAnimState}");
        }

        // setup audio
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;
        audioSource.loop = true;
        audioSource.volume = roamVolume;

        if (roamSound)
        {
            audioSource.clip = roamSound;
        }

        // diam dulu
        agent.isStopped = true;
        if (hasAnimator)
        {
            animator.speed = 1f;
            if (!string.IsNullOrEmpty(detectedAnimState))
                animator.Play("idle", 0, 0); // fallback: idle dulu
        }
    }

    void Update()
    {
        if (!isActive)
        {
            CheckPlayerDistance();
            return;
        }

        roamTimer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (roamTimer >= roamInterval)
            {
                RoamToRandomPoint();
                roamTimer = 0f;
            }
        }

        if (hasAnimator && useAnimation && !string.IsNullOrEmpty(detectedAnimState))
        {
            float speedPercent = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("Speed", speedPercent);
            animator.SetBool(detectedAnimState, speedPercent > 0.1f);
        }

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= activationDistance)
        {
            isActive = true;
            agent.isStopped = false;
            agent.speed = moveSpeed;
            RoamToRandomPoint();

            if (roamSound && !audioSource.isPlaying)
                audioSource.Play();

            if (hasAnimator && !string.IsNullOrEmpty(detectedAnimState))
                animator.SetBool(detectedAnimState, true);
        }
    }

    void RoamToRandomPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * roamRadius;
        randomDir += spawnCenter;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    string DetectWalkAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return "";

        // ambil semua state di Base Layer
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            string name = clip.name.ToLower();
            if (name.Contains("walk") || name.Contains("run") || name.Contains("move"))
                return clip.name; // ambil nama animasi yang cocok
        }
        return ""; // tidak ketemu
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnCenter == Vector3.zero ? transform.position : spawnCenter, roamRadius);
        Gizmos.color = Color.red;
        if (player != null)
            Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}
