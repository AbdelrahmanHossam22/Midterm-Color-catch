using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player; // Auto-assigns itself if left empty
    private NavMeshAgent agent;

    [Header("Chase Settings")]
    [Tooltip("How often to update path (seconds). Lower = smoother chase.")]
    public float repathInterval = 0.05f;

    private float repathTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        agent.autoBraking = false;
    }

    void Update()
    {
        if (!player || !agent || !agent.isOnNavMesh) return;

        repathTimer += Time.deltaTime;
        if (repathTimer < repathInterval) return;
        repathTimer = 0f;

        // Always follow player
        agent.SetDestination(player.position);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            GameManager.Instance.HandleEnemyCatch();
        }
    }
}
