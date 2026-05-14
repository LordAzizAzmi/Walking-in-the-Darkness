using UnityEngine;
using UnityEngine.AI;

public class HantuAI : MonoBehaviour
{
    [Header("Target & Movement")]
    public Transform target;
    public float detectDistance = 10f;
    public float attackDistance = 1.5f;
    public float chaseSpeed = 3f;
    public float roamSpeed = 1.5f;
    public float roamRadius = 10f;
    public float roamInterval = 5f;

    [Header("Sounds")]
    public AudioClip roamSound;
    public AudioClip chaseSound;
    public AudioClip attackSound;
    public float roamSoundInterval = 15f;

    [Header("Chase Tuning")]
    public float pathUpdateInterval = 0.3f;
    public float minDistanceToUpdatePath = 1f;

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    private float roamTimer;
    private float roamSoundTimer;
    private Vector3 roamDestination;
    private bool isChasing = false;
    private bool isAttacking = false;

    // tambahan untuk chase dari kode1
    private float lastPathUpdateTime = 0f;
    private Vector3 lastDestination;
    private Vector3 lastTargetPos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Set audio 3D
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // Agent defaults
        agent.stoppingDistance = attackDistance;
        agent.autoBraking = false;
        agent.speed = roamSpeed;

        roamTimer = roamInterval;
        roamSoundTimer = roamSoundInterval;

        lastDestination = transform.position;
        lastTargetPos = target != null ? target.position : Vector3.zero;
    }

    void Update()
    {
        if (target == null || agent == null) return;

        // hitung jarak & prediksi kecepatan target (digunakan untuk update path)
        Vector3 playerVelocity = (target.position - lastTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastTargetPos = target.position;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackDistance)
        {
            AttackPlayer();
        }
        else if (distance <= detectDistance)
        {
            ChasePlayer_WithPathUpdates(playerVelocity);
        }
        else
        {
            RoamRandomly();
        }

        // Update animasi jalan
        if (anim != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    // -------------------
    // === ROAMING ===
    // -------------------
    void RoamRandomly()
    {
        agent.speed = roamSpeed;
        roamTimer += Time.deltaTime;
        roamSoundTimer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance < 0.5f && roamTimer >= roamInterval)
        {
            roamDestination = GetRandomNavmeshLocation(roamRadius);
            agent.SetDestination(roamDestination);
            roamTimer = 0f;
        }

        if (roamSound != null && roamSoundTimer >= roamSoundInterval)
        {
            audioSource.PlayOneShot(roamSound);
            roamSoundTimer = 0f;
        }

        // reset chase
        if (isChasing)
        {
            StopChaseSound();
            isChasing = false;
        }

        if (anim != null)
            anim.ResetTrigger("Attack");
    }

    // -------------------
    // === CHASE (dari kode1, dipakai di kode2) ===
    // -------------------
    void ChasePlayer_WithPathUpdates(Vector3 playerVelocity)
    {
        if (target == null) return;

        agent.speed = chaseSpeed;
        agent.isStopped = false;

        // pertama kali mulai chase: segera set destination
        if (!isChasing)
        {
            SetChaseDestination(target.position);
            PlayChaseSound();
            isChasing = true;
        }
        else
        {
            // update path periodik atau jika player berpindah lebih dari minDistanceToUpdatePath
            if (Time.time >= lastPathUpdateTime + pathUpdateInterval ||
                Vector3.Distance(target.position, lastDestination) > minDistanceToUpdatePath)
            {
                SetChaseDestination(GetPredictedPosition(playerVelocity));
            }
        }

        if (anim != null)
            anim.ResetTrigger("Attack");

        isAttacking = false;
    }

    Vector3 GetPredictedPosition(Vector3 playerVelocity)
    {
        // prediksi sederhana: jika bergerak, offset sedikit
        return playerVelocity.magnitude > 0.1f
            ? target.position + Vector3.Lerp(playerVelocity * 0.5f, Vector3.zero, 0.5f)
            : target.position;
    }

    void SetChaseDestination(Vector3 targetPos)
    {
        lastDestination = targetPos;
        lastPathUpdateTime = Time.time;
        agent.SetDestination(targetPos);
    }

    // -------------------
    // === ATTACK ===
    // -------------------
    void AttackPlayer()
    {
        if (isAttacking) return;
        isAttacking = true;

        agent.isStopped = true;

        // Mainkan animasi Attack (jika ada)
        if (anim != null)
            anim.SetTrigger("Attack");

        // Mainkan suara serangan
        if (attackSound != null)
            audioSource.PlayOneShot(attackSound);

        // Kembali ke chase setelah 2 detik (atau sesuai kebutuhan)
        Invoke(nameof(ResetAttack), 2f);
    }

    void ResetAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    // -------------------
    // === NAVMESH ===
    // -------------------
    Vector3 GetRandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    // -------------------
    // === AUDIO ===
    // -------------------
    void PlayChaseSound()
    {
        if (chaseSound != null)
        {
            // hanya set clip jika belum sama, supaya tidak restart terus
            if (audioSource.clip != chaseSound)
            {
                audioSource.clip = chaseSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    void StopChaseSound()
    {
        if (audioSource.isPlaying && audioSource.clip == chaseSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    // -------------------
    // === DEBUG GIZMOS ===
    // -------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, roamRadius);
    }
}
