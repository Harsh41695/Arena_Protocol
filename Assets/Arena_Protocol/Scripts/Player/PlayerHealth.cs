using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 2f;

    public float MaxHealth => maxHealth;

    public NetworkVariable<float> CurrentHealth =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> IsDead =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public event Action<float, float> OnHealthChanged;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        CurrentHealth.OnValueChanged += HandleHealthChanged;

        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            IsDead.Value = false;
        }

        OnHealthChanged?.Invoke(
            CurrentHealth.Value,
            maxHealth
        );

        if (IsOwner)
        {
            GameplayHUD.Instance?.InitializeLocalPlayer(
                this,
                GetComponent<DashAbility>(),
                GetComponent<ProjectileAbility>(),
                GetComponent<HealAbility>()
            );
        }
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(
        float previousValue,
        float newValue)
    {
        OnHealthChanged?.Invoke(
            newValue,
            maxHealth
        );
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer ||
            damage <= 0f ||
            IsDead.Value)
        {
            return;
        }

        CurrentHealth.Value =
            Mathf.Max(
                CurrentHealth.Value - damage,
                0f
            );

        if (CurrentHealth.Value <= 0f)
            HandleDeath();
    }

    public void Heal(float amount)
    {
        if (!IsServer ||
            amount <= 0f ||
            IsDead.Value)
        {
            return;
        }

        CurrentHealth.Value =
            Mathf.Min(
                CurrentHealth.Value + amount,
                maxHealth
            );
    }

    public void RestoreHealth(float health)
    {
        if (!IsServer)
            return;

        CurrentHealth.Value =
            Mathf.Clamp(health, 0f, maxHealth);

        IsDead.Value =
            CurrentHealth.Value <= 0f;
    }

    private void HandleDeath()
    {
        if (!IsServer || IsDead.Value)
            return;

        IsDead.Value = true;

        Debug.Log(
            $"{name} died. Respawning in {respawnDelay} seconds."
        );

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (!IsSpawned)
            yield break;

        Vector3 spawnPosition =
            ReconnectionManager.Instance != null
                ? ReconnectionManager.Instance.GetSpawnPosition(
                    OwnerClientId)
                : Vector3.zero;

        RespawnOwnerRpc(spawnPosition);

        CurrentHealth.Value = maxHealth;
        IsDead.Value = false;
    }

    [Rpc(SendTo.Owner)]
    private void RespawnOwnerRpc(Vector3 spawnPosition)
    {
        if (characterController != null)
            characterController.enabled = false;

        transform.position = spawnPosition;

        if (characterController != null)
            characterController.enabled = true;
    }
}