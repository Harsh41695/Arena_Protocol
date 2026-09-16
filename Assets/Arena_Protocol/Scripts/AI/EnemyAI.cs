using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : NetworkBehaviour
{
    public enum EnemyState : byte
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float loseTargetRange = 14f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.25f;

    [Header("Attack Visual")]
    [SerializeField] private GameObject attackTelegraph;
    [SerializeField] private GameObject attackVFX;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float patrolWaitTime = 1.5f;
    [SerializeField] private float destinationThreshold = 0.5f;

    public NetworkVariable<EnemyState> CurrentState =
        new NetworkVariable<EnemyState>(
            EnemyState.Patrol,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<ulong> TargetClientId =
        new NetworkVariable<ulong>(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private NavMeshAgent agent;
    private PlayerHealth currentTarget;

    private Vector3 spawnPosition;

    private float nextAttackTime;
    private float patrolWaitTimer;

    private bool isTelegraphActive;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;
    }


    public override void OnNetworkSpawn()
    {
        // Important for pooled enemies.
        isTelegraphActive = false;

        if (attackTelegraph != null)
            attackTelegraph.SetActive(false);

        if (attackVFX != null)
            attackVFX.SetActive(false);

        if (!IsServer)
        {
            agent.enabled = false;
            return;
        }

        SetState(EnemyState.Patrol);
        SetNewPatrolDestination();
    }


    public override void OnNetworkDespawn()
    {
        isTelegraphActive = false;

        if (attackTelegraph != null)
            attackTelegraph.SetActive(false);

        if (attackVFX != null)
            attackVFX.SetActive(false);

        currentTarget = null;

        base.OnNetworkDespawn();
    }


    private void Update()
    {
        if (!IsServer)
            return;

        switch (CurrentState.Value)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;

            case EnemyState.Attack:
                UpdateAttack();
                break;
        }
    }


    private void UpdatePatrol()
    {
        PlayerHealth target = FindClosestPlayer();

        if (target != null)
        {
            SetTarget(target);
            SetState(EnemyState.Chase);
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > destinationThreshold)
            return;

        patrolWaitTimer += Time.deltaTime;

        if (patrolWaitTimer >= patrolWaitTime)
        {
            patrolWaitTimer = 0f;
            SetNewPatrolDestination();
        }
    }


    private void UpdateChase()
    {
        if (!IsTargetValid())
        {
            SetTelegraph(false);

            ClearTarget();
            SetState(EnemyState.Patrol);
            SetNewPatrolDestination();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                currentTarget.transform.position
            );

        if (distance > loseTargetRange)
        {
            SetTelegraph(false);

            ClearTarget();
            SetState(EnemyState.Patrol);
            SetNewPatrolDestination();
            return;
        }

        if (distance <= attackRange)
        {
            agent.ResetPath();

            SetState(EnemyState.Attack);

            // Turn attack range indicator ON immediately.
            SetTelegraph(true);

            return;
        }

        agent.SetDestination(currentTarget.transform.position);
    }


    private void UpdateAttack()
    {
        if (!IsTargetValid())
        {
            SetTelegraph(false);

            ClearTarget();
            SetState(EnemyState.Patrol);
            SetNewPatrolDestination();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                currentTarget.transform.position
            );

        if (distance > loseTargetRange)
        {
            SetTelegraph(false);

            ClearTarget();
            SetState(EnemyState.Patrol);
            SetNewPatrolDestination();
            return;
        }

        if (distance > attackRange)
        {
            SetTelegraph(false);

            SetState(EnemyState.Chase);
            return;
        }

        // Player remains inside attack range.
        SetTelegraph(true);

        FaceTarget();

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime =
                Time.time + attackCooldown;

            // Server-authoritative damage.
            currentTarget.TakeDamage(attackDamage);

            // Visual attack effect on everyone.
            PlayAttackRpc();
        }
    }



    private void SetTelegraph(bool active)
    {
        // Prevent sending an RPC every frame.
        if (isTelegraphActive == active)
            return;

        isTelegraphActive = active;

        SetTelegraphRpc(active);
    }


    [Rpc(SendTo.Everyone)]
    private void SetTelegraphRpc(bool active)
    {
        if (attackTelegraph != null)
            attackTelegraph.SetActive(active);
    }



    [Rpc(SendTo.Everyone)]
    private void PlayAttackRpc()
    {
        if (attackVFX != null)
            attackVFX.SetActive(true);
        Debug.Log("Effect Called");
    }

    private PlayerHealth FindClosestPlayer()
    {
        PlayerHealth closestPlayer = null;

        float closestDistanceSqr =
            detectionRange * detectionRange;

        foreach (var client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerHealth health =
                client.PlayerObject.GetComponent<PlayerHealth>();

            if (health == null ||
                health.CurrentHealth.Value <= 0f)
                continue;

            Vector3 difference =
                health.transform.position -
                transform.position;

            float distanceSqr =
                difference.sqrMagnitude;

            if (distanceSqr <= closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestPlayer = health;
            }
        }

        return closestPlayer;
    }


    private bool IsTargetValid()
    {
        return currentTarget != null &&
               currentTarget.IsSpawned &&
               currentTarget.CurrentHealth.Value > 0f;
    }


    private void SetTarget(PlayerHealth target)
    {
        currentTarget = target;

        TargetClientId.Value =
            target.OwnerClientId;
    }


    private void ClearTarget()
    {
        currentTarget = null;

        TargetClientId.Value =
            ulong.MaxValue;
    }


    private void SetState(EnemyState newState)
    {
        if (CurrentState.Value == newState)
            return;

        CurrentState.Value = newState;
    }


    private void SetNewPatrolDestination()
    {
        Vector2 randomCircle =
            Random.insideUnitCircle * patrolRadius;

        Vector3 randomPosition =
            spawnPosition +
            new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            );

        if (NavMesh.SamplePosition(
                randomPosition,
                out NavMeshHit hit,
                patrolRadius,
                NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void FaceTarget()
    {
        Vector3 direction =
            currentTarget.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                agent.angularSpeed * Time.deltaTime
            );
    }
}