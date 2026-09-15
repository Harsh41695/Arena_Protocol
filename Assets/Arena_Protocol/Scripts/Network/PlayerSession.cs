using Unity.Netcode;
using UnityEngine;

public class PlayerSession : NetworkBehaviour
{
    private PlayerHealth playerHealth;
    private DashAbility dashAbility;
    private ProjectileAbility projectileAbility;
    private HealAbility healAbility;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        dashAbility = GetComponent<DashAbility>();
        projectileAbility = GetComponent<ProjectileAbility>();
        healAbility = GetComponent<HealAbility>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        ReconnectionManager.Instance?.RestorePlayerState(
            OwnerClientId,
            this
        );
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            ReconnectionManager.Instance?.SavePlayerState(
                OwnerClientId,
                this
            );
        }
    }

    public PlayerSessionState CaptureState(string playerId)
    {
        return new PlayerSessionState
        {
            PlayerId = playerId,

            Health = playerHealth.CurrentHealth.Value,

            DashCooldownEndTime =
                dashAbility.CooldownEndTime,

            ProjectileCooldownEndTime =
                projectileAbility.CooldownEndTime,

            HealCooldownEndTime =
                healAbility.CooldownEndTime
        };
    }

    public void RestoreState(PlayerSessionState state)
    {
        if (!IsServer || state == null)
            return;

        playerHealth.RestoreHealth(state.Health);

        dashAbility.RestoreCooldownEndTime(
            state.DashCooldownEndTime
        );

        projectileAbility.RestoreCooldownEndTime(
            state.ProjectileCooldownEndTime
        );

        healAbility.RestoreCooldownEndTime(
            state.HealCooldownEndTime
        );
    }
}