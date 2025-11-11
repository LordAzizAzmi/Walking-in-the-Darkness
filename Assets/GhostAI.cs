using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public Transform target; // Player XR Rig
    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Misalnya aktifkan jumpscare atau efek suara
            Debug.Log("Player ketangkap!");
        }
    }
}
