using Unity.Netcode;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    [Header("Ability")]
    [SerializeField] protected float cooldownDuration = 3f;
    protected PlayerHealth playerHealth;

    protected virtual void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    protected NetworkVariable<float> cooldownEndTime =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public float CooldownDuration => cooldownDuration;

    public bool IsOnCooldown
    {
        get
        {
            return NetworkManager.Singleton.ServerTime.Time < cooldownEndTime.Value;
        }
    }

    public float RemainingCooldown
    {
        get
        {
            double remaining =
                cooldownEndTime.Value -
                NetworkManager.Singleton.ServerTime.Time;

            return Mathf.Max(0f, (float)remaining);
        }
    }

    public float CooldownEndTime => cooldownEndTime.Value;

    public void RestoreCooldownEndTime(float endTime)
    {
        if (!IsServer)
            return;

        cooldownEndTime.Value = endTime;
    }

    public void TryUseAbility()
    {
        if (!IsOwner)
            return;

        if (playerHealth != null &&
            playerHealth.IsDead.Value)
            return;

        RequestUseAbilityRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestUseAbilityRpc()
    {
        if (playerHealth != null &&
            playerHealth.IsDead.Value)
            return;

        if (IsOnCooldown)
            return;

        if (!CanUseAbility())
            return;

        ExecuteAbility();

        cooldownEndTime.Value =
            (float)NetworkManager.Singleton.ServerTime.Time +
            cooldownDuration;
    }

    protected virtual bool CanUseAbility()
    {
        return true;
    }

    protected abstract void ExecuteAbility();
}