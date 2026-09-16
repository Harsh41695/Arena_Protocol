using System.Collections.Generic;
using UnityEngine;

public class VFXPool : MonoBehaviour
{
    public static VFXPool Instance { get; private set; }

    [System.Serializable]
    public class VFXPoolEntry
    {
        public string id;
        public GameObject prefab;
        [Min(1)] public int poolSize = 5;

        [HideInInspector]
        public List<GameObject> instances = new();
    }

    [SerializeField]
    private List<VFXPoolEntry> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (VFXPoolEntry pool in pools)
        {
            if (pool.prefab == null)
                continue;

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject instance =
                    Instantiate(pool.prefab, transform);

                instance.SetActive(false);

                pool.instances.Add(instance);
            }
        }
    }

    public void Play(
        string id,
        Vector3 position,
        Quaternion rotation)
    {
        VFXPoolEntry pool = GetPool(id);

        if (pool == null)
            return;

        for (int i = 0; i < pool.instances.Count; i++)
        {
            GameObject effect = pool.instances[i];

            if (effect.activeSelf)
                continue;

            effect.transform.SetPositionAndRotation(
                position,
                rotation
            );

            effect.SetActive(true);

            return;
        }

        Debug.LogWarning(
            $"VFX Pool '{id}' has no available effect."
        );
    }

    private VFXPoolEntry GetPool(string id)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].id == id)
                return pools[i];
        }

        return null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}