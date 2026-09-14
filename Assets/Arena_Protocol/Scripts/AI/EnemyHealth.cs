using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    public float MaxHealth => maxHealth;

    public NetworkVariable<float> CurrentHealth =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public event Action<float, float> OnHealthChanged;

    public override void OnNetworkSpawn()
    {
        CurrentHealth.OnValueChanged += HandleHealthChanged;

        if (IsServer)
            CurrentHealth.Value = maxHealth;

        OnHealthChanged?.Invoke(CurrentHealth.Value, maxHealth);
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer || damage <= 0f)
            return;

        CurrentHealth.Value =
            Mathf.Max(0f, CurrentHealth.Value - damage);

        if (CurrentHealth.Value <= 0f)
            Die();
    }

    private void HandleHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue, maxHealth);
    }

    private void Die()
    {
        if (!IsServer)
            return;

        NetworkObject.Despawn();
    }
}