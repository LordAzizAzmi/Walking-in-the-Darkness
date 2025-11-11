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

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    private float roamTimer;
    private float roamSoundTimer;
    private Vector3 roamDestination;
    private bool isChasing = false;
    private bool isAttacking = false;

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

        agent.stoppingDistance = 0.5f;
        roamTimer = roamInterval;
        roamSoundTimer = roamSoundInterval;
    }

    void Update()
    {
        if (target == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackDistance)
        {
            AttackPlayer();
        }
        else if (distance <= detectDistance)
        {
            ChasePlayer();
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
    // === BEHAVIOR ===
    // -------------------

    void RoamRandomly()
    {
        agent.speed = roamSpeed;
        roamTimer += Time.deltaTime;
        roamSoundTimer += Time.deltaTime;

        // Bergerak acak
        if (!agent.pathPending && agent.remainingDistance < 0.5f && roamTimer >= roamInterval)
        {
            roamDestination = GetRandomNavmeshLocation(roamRadius);
            agent.SetDestination(roamDestination);
            roamTimer = 0f;
        }

        // Suara roaming
        if (roamSound != null && roamSoundTimer >= roamSoundInterval)
        {
            audioSource.PlayOneShot(roamSound);
            roamSoundTimer = 0f;
        }

        // Reset state
        if (isChasing)
        {
            StopChaseSound();
            isChasing = false;
        }

        if (anim != null)
            anim.ResetTrigger("Attack");
    }

    void ChasePlayer()
    {
        if (target == null) return;

        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        if (!isChasing)
        {
            PlayChaseSound();
            isChasing = true;
        }

        if (anim != null)
            anim.ResetTrigger("Attack");

        isAttacking = false;
    }

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

        // Kembali ke chase setelah 2 detik
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
            audioSource.clip = chaseSound;
            audioSource.loop = true;
            audioSource.Play();
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
