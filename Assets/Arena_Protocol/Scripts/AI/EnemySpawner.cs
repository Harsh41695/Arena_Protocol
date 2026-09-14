using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private int startingEnemyCount = 2;
    [SerializeField] private int additionalEnemiesPerWave = 1;
    [SerializeField] private float timeBetweenWaves = 3f;

    public NetworkVariable<int> CurrentWave =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> AliveEnemies =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private readonly List<NetworkObject> spawnedEnemies =
        new List<NetworkObject>();

    private Coroutine waveRoutine;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        waveRoutine = StartCoroutine(StartNextWaveRoutine());
    }

    public override void OnNetworkDespawn()
    {
        if (waveRoutine != null)
            StopCoroutine(waveRoutine);

        spawnedEnemies.Clear();
    }

    private IEnumerator StartNextWaveRoutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        StartNextWave();
    }

    private void StartNextWave()
    {
        CurrentWave.Value++;

        int enemyCount =
            startingEnemyCount +
            (CurrentWave.Value - 1) * additionalEnemiesPerWave;

        AliveEnemies.Value = enemyCount;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(i);
        }
    }

    private void SpawnEnemy(int index)
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform spawnPoint =
            spawnPoints[index % spawnPoints.Length];

        NetworkObject enemy =
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        enemy.Spawn();

        spawnedEnemies.Add(enemy);

        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDied += HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(EnemyHealth enemyHealth)
    {
        if (!IsServer)
            return;

        enemyHealth.OnEnemyDied -= HandleEnemyDied;

        spawnedEnemies.Remove(enemyHealth.NetworkObject);

        AliveEnemies.Value =
            Mathf.Max(0, AliveEnemies.Value - 1);

        if (AliveEnemies.Value == 0)
        {
            waveRoutine = StartCoroutine(StartNextWaveRoutine());
        }
    }
}