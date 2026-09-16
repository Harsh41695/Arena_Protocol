using Unity.Netcode;
using UnityEngine;

public class HealAbility : AbilityBase
{
    [Header("Heal")]
    [SerializeField] private float healAmount = 25f;

    [Header("Heal Visual")]
    [SerializeField] private GameObject healVFX;

    protected override bool CanUseAbility()
    {
        if (playerHealth == null)
            return false;

        return !playerHealth.IsDead.Value &&
               playerHealth.CurrentHealth.Value > 0f &&
               playerHealth.CurrentHealth.Value < playerHealth.MaxHealth;
    }

    protected override void ExecuteAbility()
    {
        playerHealth.Heal(healAmount);

        PlayHealVFXRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void PlayHealVFXRpc()
    {
        if (healVFX != null)
            healVFX.SetActive(true);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (healVFX != null)
            healVFX.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        if (healVFX != null)
            healVFX.SetActive(false);

        base.OnNetworkDespawn();
    }
}