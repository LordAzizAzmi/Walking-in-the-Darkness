using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class HantuHide : MonoBehaviour
{
    public enum State { Roaming, Investigate, Chase, Search }
    public State currentState = State.Roaming;


[Header("References")]
    public Transform player; // Assign di inspector atau auto-detect
    NavMeshAgent agent;
    AudioSource audioSource;
    public Animator animator; // Optional

    [Header("Perception")]
    public float viewDistance = 20f;
    [Range(0, 360)] public float viewAngle = 120f;
    public LayerMask obstacleMask; // Layer untuk obstacles (dinding)
    public string playerTag = "Player";

    [Header("Roaming / Patrol")]
    public Transform[] patrolPoints; // Kosong = random wandering
    int patrolIndex = 0;
    public float roamRadius = 30f;
    public float patrolPointTolerance = 1f;

    [Header("Chase / Search")]
    public float chaseSpeed = 6f;
    public float roamSpeed = 3.5f;
    public float searchDuration = 8f;
    public float investigationDelay = 0.2f;

    [Header("Memory / Sound")]
    public Vector3 lastKnownPlayerPosition;
    public float timeSinceSeen = Mathf.Infinity;
    public float loseSightTime = 1.2f;

    [Header("Audio")]
    public AudioClip ambientRoam;
    public AudioClip footstepClip;
    public AudioClip chaseScream;
    public AudioClip investigateSound;

    bool playerInFOV = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }

        agent.speed = roamSpeed;
        agent.stoppingDistance = 0.5f;
    }

    void Start()
    {
        if (ambientRoam)
        {
            audioSource.clip = ambientRoam;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
            SetDestinationToPatrolPoint();
        else
            SetRandomRoamDestination();

        StartCoroutine(FootstepLoop());
    }

    void Update()
    {
        playerInFOV = CheckPlayerInFOV();

        switch (currentState)
        {
            case State.Roaming:
                DoRoaming();
                break;
            case State.Chase:
                DoChase();
                break;
        }

        if (playerInFOV)
        {
            lastKnownPlayerPosition = player.position;
            timeSinceSeen = 0f;
            if (currentState != State.Chase)
                StartChase();
        }
        else
        {
            timeSinceSeen += Time.deltaTime;
            if (currentState == State.Chase && timeSinceSeen >= loseSightTime)
                StartCoroutine(StartSearchCoroutine());
        }

        if (animator)
        {
            animator.SetBool("isChasing", currentState == State.Chase);
            animator.SetFloat("speed", agent.velocity.magnitude / chaseSpeed);

        }
    }

    void DoRoaming()
    {
        agent.speed = roamSpeed;
        if (!agent.pathPending && agent.remainingDistance <= patrolPointTolerance)
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                SetDestinationToPatrolPoint();
            }
            else
            {
                SetRandomRoamDestination();
            }
        }
    }

    void DoChase()
    {
        agent.speed = chaseSpeed;
        if (player != null && agent.isOnNavMesh)
            agent.SetDestination(player.position);

        if (chaseScream && !audioSource.isPlaying)
            audioSource.PlayOneShot(chaseScream);
    }

    void StartChase()
    {
        currentState = State.Chase;
        agent.acceleration = 24f;
        if (chaseScream) audioSource.PlayOneShot(chaseScream);
    }

    IEnumerator StartSearchCoroutine()
    {
        currentState = State.Search;
        agent.SetDestination(lastKnownPlayerPosition);
        agent.speed = roamSpeed;

        while (agent.pathPending) yield return null;
        while (agent.remainingDistance > patrolPointTolerance)
            yield return null;

        float t = 0f;
        float radius = 6f;
        int steps = 6;
        int i = 0;

        while (t < searchDuration)
        {
            float angle = (i / (float)steps) * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 point = lastKnownPlayerPosition + offset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                while (agent.pathPending) yield return null;
                while (agent.remainingDistance > patrolPointTolerance)
                {
                    if (CheckPlayerInFOV()) yield break;
                    yield return null;
                }
            }

            i = (i + 1) % steps;
            t += 1f;
            yield return new WaitForSeconds(0.5f);
        }

        currentState = State.Roaming;
        if (patrolPoints != null && patrolPoints.Length > 0) SetDestinationToPatrolPoint(); else SetRandomRoamDestination();
    }

    bool CheckPlayerInFOV()
    {
        if (player == null) return false;
        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;
        if (dist > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angleToPlayer > viewAngle * 0.5f) return false;

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        if (Physics.Raycast(origin, (target - origin).normalized, out hit, viewDistance, ~0))
        {
            if (hit.transform.CompareTag(playerTag)) return true;
        }
        return false;
    }

    void SetDestinationToPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (agent.isOnNavMesh) agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void SetRandomRoamDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * roamRadius;
        randomDir += transform.position;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDir, out navHit, roamRadius, NavMesh.AllAreas))
        {
            if (agent.isOnNavMesh) agent.SetDestination(navHit.position);
        }
    }

    IEnumerator FootstepLoop()
    {
        while (true)
        {
            if (agent.velocity.magnitude > 0.5f && footstepClip != null)
            {
                audioSource.PlayOneShot(footstepClip);
                float interval = 0.5f / (agent.velocity.magnitude / roamSpeed);
                yield return new WaitForSeconds(interval);
            }
            else yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewDistance;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward * viewDistance;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.color = Color.red;
        if (currentState == State.Chase) Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }

}
