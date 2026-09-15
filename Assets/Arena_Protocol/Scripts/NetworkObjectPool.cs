using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectPool : MonoBehaviour
{
    public static NetworkObjectPool Instance { get; private set; }

    [System.Serializable]
    public class PooledPrefab
    {
        public NetworkObject prefab;

        [Min(1)]
        public int prewarmCount = 5;
    }

    [SerializeField] private PooledPrefab[] pooledPrefabs;

    private readonly Dictionary<GameObject, Queue<NetworkObject>> pools = new();
    private readonly Dictionary<GameObject, PooledObjectHandler> handlers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        foreach (PooledPrefab entry in pooledPrefabs)
        {
            if (entry.prefab == null)
                continue;

            GameObject prefabObject = entry.prefab.gameObject;

            if (pools.ContainsKey(prefabObject))
                continue;

            Queue<NetworkObject> pool = new();
            pools.Add(prefabObject, pool);

            PooledObjectHandler handler =
                new PooledObjectHandler(entry.prefab, pool);

            handlers.Add(prefabObject, handler);

            NetworkManager.Singleton.PrefabHandler.AddHandler(
                prefabObject,
                handler
            );

            if (!NetworkManager.Singleton.IsServer)
                continue;

            for (int i = 0; i < entry.prewarmCount; i++)
            {
                NetworkObject instance =
                    Instantiate(entry.prefab);

                instance.gameObject.SetActive(false);
                pool.Enqueue(instance);
            }
        }
    }

    public NetworkObject Get(
        NetworkObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (!NetworkManager.Singleton.IsServer)
            return null;

        if (!pools.TryGetValue(
                prefab.gameObject,
                out Queue<NetworkObject> pool))
        {
            Debug.LogError(
                $"{prefab.name} is not registered in NetworkObjectPool."
            );

            return null;
        }

        NetworkObject instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();

            instance.transform.SetPositionAndRotation(
                position,
                rotation
            );

            instance.gameObject.SetActive(true);
        }
        else
        {
            instance = Instantiate(
                prefab,
                position,
                rotation
            );
        }

        return instance;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (NetworkManager.Singleton == null)
            return;

        foreach (var pair in handlers)
        {
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(
                pair.Key
            );
        }
    }

    private class PooledObjectHandler : INetworkPrefabInstanceHandler
    {
        private readonly NetworkObject prefab;
        private readonly Queue<NetworkObject> pool;

        public PooledObjectHandler(
            NetworkObject prefab,
            Queue<NetworkObject> pool)
        {
            this.prefab = prefab;
            this.pool = pool;
        }

        public NetworkObject Instantiate(
            ulong ownerClientId,
            Vector3 position,
            Quaternion rotation)
        {
            NetworkObject instance;

            if (pool.Count > 0)
            {
                instance = pool.Dequeue();

                instance.transform.SetPositionAndRotation(
                    position,
                    rotation
                );

                instance.gameObject.SetActive(true);
            }
            else
            {
                instance = Object.Instantiate(
                    prefab,
                    position,
                    rotation
                );
            }

            return instance;
        }

        public void Destroy(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(false);
            pool.Enqueue(networkObject);
        }
    }
}