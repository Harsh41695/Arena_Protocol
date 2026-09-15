using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : AbilityBase
{
    [Header("Projectile")]
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;

    private Camera mainCamera;
    private PlayerHealth health;

    protected override void Awake()
    {
        base.Awake();

        health = GetComponent<PlayerHealth>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
            mainCamera = Camera.main;
    }

    public void TryUseProjectile()
    {
        if (!IsOwner)
            return;

        // Dead player cannot shoot.
        if (health != null && health.IsDead.Value)
            return;

        if (IsOnCooldown)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Ray ray =
            mainCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane =
            new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 targetPoint = ray.GetPoint(distance);

        RequestProjectileRpc(targetPoint);
    }

    [Rpc(SendTo.Server)]
    private void RequestProjectileRpc(Vector3 targetPoint)
    {
        // Server validation too.
        if (health != null && health.IsDead.Value)
            return;

        if (IsOnCooldown)
            return;

        if (!CanUseAbility())
            return;

        Vector3 direction =
            targetPoint - spawnPoint.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        SpawnProjectile(direction);

        cooldownEndTime.Value =
            (float)NetworkManager.Singleton.ServerTime.Time +
            cooldownDuration;
    }

    private void SpawnProjectile(Vector3 direction)
    {
        Quaternion rotation =
            Quaternion.LookRotation(direction, Vector3.up);

        NetworkObject projectile =
            NetworkObjectPool.Instance.Get(
                projectilePrefab,
                spawnPoint.position,
                rotation
            );

        if (projectile == null)
            return;

        projectile.Spawn();
    }

    protected override bool CanUseAbility()
    {
        return projectilePrefab != null &&
               spawnPoint != null &&
               NetworkObjectPool.Instance != null;
    }

    protected override void ExecuteAbility()
    {
        // Not used because projectile requires cursor target data.
    }
}