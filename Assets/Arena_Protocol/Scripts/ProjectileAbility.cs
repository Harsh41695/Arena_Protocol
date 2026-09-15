using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : AbilityBase
{
    [Header("Projectile")]
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;

    protected override bool CanUseAbility()
    {
        return projectilePrefab != null &&
               spawnPoint != null &&
               NetworkObjectPool.Instance != null;
    }

    protected override void ExecuteAbility()
    {
        NetworkObject projectile =
            NetworkObjectPool.Instance.Get(
                projectilePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        if (projectile == null)
            return;

        projectile.Spawn();
    }
}