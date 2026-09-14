using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        NetworkObject enemy =
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        enemy.Spawn();
    }
}