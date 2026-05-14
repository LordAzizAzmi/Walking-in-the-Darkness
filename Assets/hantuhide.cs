using UnityEngine;
using UnityEngine.AI;

public class HantuHide : MonoBehaviour
{
    public enum State { Idle, Roam, Chase, Attack, LostSight }
    public State currentState = State.Idle;

    [Header("Target & Movement")]
    public Transform target;
    public Transform targetRoot;   // XR Origin
    public Transform targetHead;   // XR Camera (player head)
    public float viewDistance = 15f;
    [Range(0, 360)] public float viewAngle = 120f;
    public LayerMask obstacleMask;
    public string playerTag = "Player";

    public float attackDistance = 1.5f;
    public float chaseSpeed = 3f;
    public float roamSpeed = 1.5f;
    public float roamRadius = 10f;
    public float roamInterval = 5f;
    public float loseSightTime = 1.5f;

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
    private bool isAttacking = false;
    private float timeSinceSeen = 999f;
    private Vector3 lastSeenPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;

        agent.stoppingDistance = 0.5f;
        currentState = State.Roam;

        roamTimer = roamInterval;
        roamSoundTimer = roamSoundInterval;
    }

    void Update()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) target = p.transform;
            else return;
        }

        bool canSee = CheckPOV();
        float dist = Vector3.Distance(transform.position, target.position);

        // ----------------------------------------
        // PRIORITAS 1 = ATTACK
        // ----------------------------------------
        if (dist <= attackDistance)
        {
            AttackPlayer();
            return;
        }

        // ----------------------------------------
        // PRIORITAS 2 = CHASE (Setiap Frame)
        // ----------------------------------------
        if (canSee)
        {
            timeSinceSeen = 0f;
            ChasePlayer();   // <-- HARUS SETIAP FRAME
            return;
        }

        // ----------------------------------------
        // LOSE SIGHT
        // ----------------------------------------
        timeSinceSeen += Time.deltaTime;

        if (timeSinceSeen < loseSightTime)
        {
            LostSight();
            return;
        }

        // ----------------------------------------
        // KEMBALI KE ROAM
        // ----------------------------------------
        RoamRandomly();

        // Set anim speed
        if (anim != null)
            anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    // ================================
    //   CHECK POV (FOV + RAYCAST)
    // ================================
    bool CheckPOV()
    {
        if (targetRoot == null) return false;

        Vector3 dir = (targetRoot.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, targetRoot.position);

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        // Spherecast untuk lebih reliable deteksi player
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * 1.2f, 0.5f, dir, out hit, viewDistance))
        {
            if (hit.transform == targetRoot || hit.transform.CompareTag(playerTag))
                return true;
        }
        return false;
    }

    // ================================
    //             CHASE
    // ================================
    void ChasePlayer()
    {
        if (currentState != State.Chase)
        {
            currentState = State.Chase;
            Debug.Log("STATE: CHASE");
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        // Prediksi posisi player untuk menghindari agent stuck
        Vector3 predictedPos = targetRoot.position;
        Rigidbody rb = targetRoot.GetComponent<Rigidbody>();
        if (rb != null) predictedPos += rb.velocity * 0.2f;

        agent.SetDestination(predictedPos);

        if (chaseSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chaseSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        isAttacking = false;
        if (anim != null) anim.ResetTrigger("Attack");
    }

    // ================================
    //            ROAMING
    // ================================
    void RoamRandomly()
    {
        if (currentState != State.Roam)
        {
            currentState = State.Roam;
            Debug.Log("STATE: ROAM");
        }

        agent.isStopped = false;
        agent.speed = roamSpeed;

        roamTimer += Time.deltaTime;
        roamSoundTimer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance < 0.5f && roamTimer >= roamInterval)
        {
            agent.SetDestination(GetRandomNavmeshLocation(roamRadius));
            roamTimer = 0f;
        }

        if (roamSound != null && roamSoundTimer >= roamSoundInterval)
        {
            audioSource.PlayOneShot(roamSound);
            roamSoundTimer = 0f;
        }
    }



    // ================================
    //           LOST SIGHT
    // ================================
    void LostSight()
    {
        if (currentState != State.LostSight)
        {
            currentState = State.LostSight;
            Debug.Log("STATE: LOST SIGHT");
        }

        agent.speed = chaseSpeed * 0.7f;
        agent.isStopped = false;
        agent.SetDestination(targetRoot.position);
    }

    // ================================
    //            ATTACK
    // ================================
    void AttackPlayer()
    {
        if (currentState != State.Attack)
        {
            currentState = State.Attack;
            Debug.Log("STATE: ATTACK");
        }

        if (isAttacking) return;
        isAttacking = true;

        agent.isStopped = true;

        if (anim != null)
            anim.SetTrigger("Attack");

        if (attackSound != null)
            audioSource.PlayOneShot(attackSound);

        Invoke(nameof(ResetAttack), 1.2f);
    }

    void ResetAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    Vector3 GetRandomNavmeshLocation(float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius + transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }
}
