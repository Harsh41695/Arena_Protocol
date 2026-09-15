using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 4f;

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
            transform.forward * speed * Time.deltaTime;

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            DespawnProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || hasHit)
            return;

        // Player projectile can only damage enemies.
        EnemyHealth enemyHealth =
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        hasHit = true;

        enemyHealth.TakeDamage(damage);

        DespawnProjectile();
    }

    private void DespawnProjectile()
    {
        if (!IsServer || !NetworkObject.IsSpawned)
            return;

        NetworkObject.Despawn();
    }
}