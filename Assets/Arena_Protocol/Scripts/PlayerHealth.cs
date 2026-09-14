using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [Header("Health")]
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
        {
            CurrentHealth.Value = maxHealth;
        }

        // Push initial value to listeners/UI.
        OnHealthChanged?.Invoke(CurrentHealth.Value, maxHealth);
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue, maxHealth);

        Debug.Log(
            $"{name} Health: {newValue}/{maxHealth}"
        );
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer)
            return;

        if (damage <= 0f)
            return;

        CurrentHealth.Value = Mathf.Max(
            CurrentHealth.Value - damage,
            0f
        );

        if (CurrentHealth.Value <= 0f)
        {
            HandleDeath();
        }
    }

    public void Heal(float amount)
    {
        if (!IsServer)
            return;

        if (amount <= 0f)
            return;

        CurrentHealth.Value = Mathf.Min(
            CurrentHealth.Value + amount,
            maxHealth
        );
    }

    private void HandleDeath()
    {
        Debug.Log($"{name} died.");

        // We'll implement death/respawn later.
    }


    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            RequestDebugDamageRpc();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            RequestDebugHealRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestDebugDamageRpc()
    {
        TakeDamage(20f);
    }

    [Rpc(SendTo.Server)]
    private void RequestDebugHealRpc()
    {
        Heal(20f);
    }
}