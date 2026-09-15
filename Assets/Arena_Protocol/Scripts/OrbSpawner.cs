using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OrbSpawner : NetworkBehaviour
{
    [Header("Orb")]
    [SerializeField] private NetworkObject orbPrefab;
    [SerializeField] private int maxOrbs = 5;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnArea = new Vector2(20f, 20f);
    [SerializeField] private float raycastHeight = 10f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 5f;

    private int activeOrbCount;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        for (int i = 0; i < maxOrbs; i++)
        {
            SpawnOrb();
        }
    }

    private void SpawnOrb()
    {
        if (!IsServer ||
            orbPrefab == null ||
            NetworkObjectPool.Instance == null)
            return;

        if (!TryGetRandomGroundPosition(out Vector3 spawnPosition))
            return;

        NetworkObject orb =
            NetworkObjectPool.Instance.Get(
                orbPrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (orb == null)
            return;

        EnergyOrb energyOrb = orb.GetComponent<EnergyOrb>();

        if (energyOrb != null)
        {
            energyOrb.OnCollected += HandleOrbCollected;
        }

        orb.Spawn();

        activeOrbCount++;
    }

    private bool TryGetRandomGroundPosition(out Vector3 position)
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX =
                Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f);

            float randomZ =
                Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f);

            Vector3 rayOrigin =
                transform.position +
                new Vector3(randomX, raycastHeight, randomZ);

            if (Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    raycastHeight * 2f,
                    groundLayer))
            {
                position = hit.point + Vector3.up * 0.5f;
                return true;
            }
        }

        position = default;
        return false;
    }

    private void HandleOrbCollected(EnergyOrb orb)
    {
        if (!IsServer)
            return;

        orb.OnCollected -= HandleOrbCollected;

        activeOrbCount =
            Mathf.Max(0, activeOrbCount - 1);

        StartCoroutine(RespawnOrbRoutine());
    }

    private IEnumerator RespawnOrbRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnOrb();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(spawnArea.x, 0.1f, spawnArea.y)
        );
    }
}