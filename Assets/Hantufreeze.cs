using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Hantufreeze : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform playerTransform;
    public Camera playerCamera;
    public float detectionRange = 20f;
    public LayerMask obstacleLayer = 1;

    [Header("Teleport Settings")]
    public float teleportDistance = 1.0f; // jarak teleport depan kamera
    public Transform playerHead; // opsional utk XR, jika punya

    [Header("Attack Recovery & Distance")]
    public float postAttackChaseDelay = 1.5f; // jeda sebelum kejar lagi
    public float minTeleportDistance = 1.2f;  // jarak minimum setelah teleport
    public float pushBackDistance = 0.5f;     // dorong hantu sedikit mundur setelah serang

    [Header("Chase & Attack")]
    public float chaseSpeed = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("Chase Tuning")]
    public float pathUpdateInterval = 0.3f;
    public float minDistanceToUpdatePath = 1f;

    [Header("Audio")]
    public AudioClip chaseSound;
    public AudioClip attackSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    // NEW: freeze player components
    [Header("Player Freeze System")]
    public MonoBehaviour playerMovementScript; // drag Player movement script here (FPS/VR locomotion)
    public float attackFreezeDuration = 1.2f;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    private bool isSeen = false;
    private bool isAttacking = false;
    private bool isChasing = false;
    private float lastAttackTime = 0f;
    private Vector3 lastPlayerPos;
    private Plane[] cameraFrustum;
    private float actualFov;
    private float lastPathUpdateTime = 0f;
    private Vector3 lastDestination;

    void Start()
    {
        DetectPlayerSetup();

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (chaseSound != null)
        {
            audioSource.clip = chaseSound;
            audioSource.loop = true;
            audioSource.volume = volume;
        }

        if (playerCamera != null)
            actualFov = playerCamera.fieldOfView;

        agent.speed = chaseSpeed;
        agent.acceleration = 20f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = attackRange;
        agent.autoBraking = false;
        lastPlayerPos = playerTransform != null ? playerTransform.position : transform.position;
        lastDestination = transform.position;
    }

    void DetectPlayerSetup()
    {
        if (playerTransform == null)
        {
            GameObject xrRig = GameObject.FindGameObjectWithTag("XR Rig") ?? GameObject.FindGameObjectWithTag("Player");
            if (xrRig != null)
            {
                playerTransform = xrRig.transform;
                playerCamera = xrRig.GetComponentInChildren<Camera>();
            }
            else
            {
                playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
                if (playerTransform == null)
                {
                    if (Camera.main != null)
                    {
                        playerTransform = Camera.main.transform.parent ?? Camera.main.transform;
                        playerCamera = Camera.main;
                    }
                }
                else
                {
                    playerCamera = playerTransform.GetComponentInChildren<Camera>() ?? Camera.main;
                }
            }
        }
        else playerCamera = playerTransform.GetComponentInChildren<Camera>() ?? Camera.main;

        if (playerTransform == null) Debug.LogError("[Hantu] Player Transform NOT FOUND!");
    }

    void Update()
    {
        if (playerTransform == null || playerCamera == null) return;

        actualFov = playerCamera.fieldOfView;
        Vector3 playerVelocity = (playerTransform.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = playerTransform.position;
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        bool prevSeen = isSeen;
        isSeen = IsPlayerSeeingMe();

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (isSeen)
        {
            if (!prevSeen)
            {
                agent.ResetPath();
                isChasing = false;
            }
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            SetAnimationIdle();
            StopChaseSound();
            if (isAttacking) isAttacking = false;
        }
        else
        {
            agent.isStopped = false;

            if (dist <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                if (isChasing)
                {
                    agent.ResetPath();
                    isChasing = false;
                }
                PerformAttack();
            }
            else if (dist <= detectionRange)
            {
                if (!isChasing)
                {
                    SetChaseDestination(playerTransform.position);
                    isChasing = true;
                }
                else if (Time.time >= lastPathUpdateTime + pathUpdateInterval ||
                         Vector3.Distance(playerTransform.position, lastDestination) > minDistanceToUpdatePath)
                {
                    SetChaseDestination(GetPredictedPosition(playerVelocity));
                }
                SetAnimationWalk();
                PlayChaseSound();
            }
            else
            {
                if (isChasing)
                {
                    agent.ResetPath();
                    isChasing = false;
                }
                agent.isStopped = true;
                agent.velocity = Vector3.Lerp(agent.velocity, Vector3.zero, Time.deltaTime * 5f);
                SetAnimationIdle();
                StopChaseSound();
            }
        }
    }

    Vector3 GetPredictedPosition(Vector3 playerVelocity)
    {
        return playerVelocity.magnitude > 0.1f
            ? playerTransform.position + Vector3.Lerp(playerVelocity * 0.5f, Vector3.zero, 0.5f)
            : playerTransform.position;
    }

    void SetChaseDestination(Vector3 targetPos)
    {
        lastDestination = targetPos;
        lastPathUpdateTime = Time.time;
        agent.SetDestination(targetPos);
    }

    bool IsPlayerSeeingMe()
    {
        Vector3 direction = transform.position - playerCamera.transform.position;
        float distance = direction.magnitude;
        if (distance > detectionRange || distance < 0.5f) return false;

        Vector3 flatForward = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;
        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

        float halfFovRad = Mathf.Deg2Rad * (actualFov / 2f);
        if (Vector3.Dot(flatForward, flatDirection) < Mathf.Cos(halfFovRad)) return false;

        Bounds ghostBounds = new Bounds(transform.position, Vector3.one * 2f);
        if (!GeometryUtility.TestPlanesAABB(cameraFrustum, ghostBounds)) return false;

        LayerMask ignoreLayers = LayerMask.GetMask("Player", "Enemy");
        RaycastHit hit;
        bool clear = !Physics.Raycast(playerCamera.transform.position, direction.normalized, out hit, distance - 0.1f, obstacleLayer & ~ignoreLayers);

        if (distance < 2f && Vector3.Dot(playerCamera.transform.forward, direction.normalized) > 0.5f) return true;

        return clear;
    }

    // ✅=== ATTACK WITH TELEPORT & FREEZE PLAYER ===✅
    void PerformAttack()
    {
        if (isAttacking) return;
        isAttacking = true;
        agent.isStopped = true;
        agent.ResetPath();

        // posisi teleport depan kamera
        Vector3 forwardPos = playerCamera.transform.position + playerCamera.transform.forward * teleportDistance;
        forwardPos.y = transform.position.y;

        // pastikan jarak minimal
        if (Vector3.Distance(forwardPos, playerTransform.position) < minTeleportDistance)
        {
            forwardPos = playerTransform.position + playerCamera.transform.forward * minTeleportDistance;
        }

        transform.position = forwardPos;
        transform.LookAt(playerTransform.position);

        FreezePlayer(true);
        SetAnimationAttack();

        if (attackSound != null)
            audioSource.PlayOneShot(attackSound, volume);

        lastAttackTime = Time.time;

        StartCoroutine(AttackSequence());
    }

    // ✅ Freeze / Unfreeze Player Controller
    void FreezePlayer(bool freeze)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = !freeze;

        // Jika kamu ingin freeze XR Head juga (opsional)
        if (playerHead != null && freeze)
        {
            // Bisa tambahkan rigidbody freeze di XR kalau perlu
        }
    }


    IEnumerator AttackSequence()
    {
        float attackDuration = 1.2f;
        yield return new WaitForSeconds(attackDuration);

        // dorong mundur sedikit biar ga tabrak player
        Vector3 backward = -transform.forward * pushBackDistance;
        transform.position += backward;

        FreezePlayer(false);

        // delay sebelum hantu bisa kejar lagi
        yield return new WaitForSeconds(postAttackChaseDelay);

        isAttacking = false;
        agent.isStopped = false;
    }



    IEnumerator FinishAttack()
    {
        yield return new WaitForSeconds(attackFreezeDuration);

        // ✅ UNFREEZE PLAYER
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        isAttacking = false;
    }

    void SetAnimationIdle()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
        animator.SetFloat("Speed", 0f);
    }

    void SetAnimationWalk()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    void SetAnimationAttack()
    {
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Attack");
        animator.SetBool("isAttacking", true);
    }

    void PlayChaseSound()
    {
        if (!audioSource.isPlaying && chaseSound != null) audioSource.Play();
    }

    void StopChaseSound()
    {
        if (audioSource.isPlaying) audioSource.Stop();
    }

    void OnGUI()
    {
        GUILayout.Label($"IsSeen: {isSeen} | IsChasing: {isChasing} | Dist: {Vector3.Distance(transform.position, playerTransform.position):F1} | Vel: {agent.velocity.magnitude:F1}");
    }
}
