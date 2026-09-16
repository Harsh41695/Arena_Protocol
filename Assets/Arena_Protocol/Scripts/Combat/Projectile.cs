using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 4f;

    [Header("Visual")]
    [SerializeField] private string hitVFXId = "ProjectileHit";

    private float remainingLifetime;
    private bool hasHit;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        remainingLifetime = lifetime;
        hasHit = false;
    }

    private void Update()
    {
        if (!IsServer || hasHit)
            return;

        transform.position +=
            transform.forward *
            speed *
            Time.deltaTime;

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
            DespawnProjectile();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || hasHit)
            return;

        EnemyHealth enemyHealth =
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        hasHit = true;

        Vector3 hitPosition =
            other.ClosestPoint(transform.position);

        enemyHealth.TakeDamage(damage);

        PlayHitVFXRpc(
            hitPosition,
            transform.rotation
        );

        DespawnProjectile();
    }

    [Rpc(SendTo.Everyone)]
    private void PlayHitVFXRpc(
        Vector3 position,
        Quaternion rotation)
    {
        VFXPool.Instance?.Play(
            hitVFXId,
            position,
            rotation
        );
    }

    private void DespawnProjectile()
    {
        if (!IsServer ||
            !NetworkObject.IsSpawned)
            return;

        NetworkObject.Despawn();
    }
}