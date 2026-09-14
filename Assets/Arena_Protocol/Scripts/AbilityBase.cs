using Unity.Netcode;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    [Header("Ability")]
    [SerializeField] protected float cooldownDuration = 3f;

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

    public void TryUseAbility()
    {
        if (!IsOwner)
            return;

        RequestUseAbilityRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestUseAbilityRpc()
    {
        if (IsOnCooldown)
            return;

        if (!CanUseAbility())
            return;

        ExecuteAbility();

        cooldownEndTime.Value =
            (float)NetworkManager.Singleton.ServerTime.Time
            + cooldownDuration;
    }

    protected virtual bool CanUseAbility()
    {
        return true;
    }

    protected abstract void ExecuteAbility();
}