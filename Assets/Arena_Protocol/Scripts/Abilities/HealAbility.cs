using UnityEngine;

public class HealAbility : AbilityBase
{
    [Header("Heal")]
    [SerializeField] private float healAmount = 25f;

   

    protected override void Awake()
    {
        base.Awake();

        playerHealth =
            GetComponent<PlayerHealth>();
    }

    protected override bool CanUseAbility()
    {
        if (playerHealth == null)
            return false;

        // Don't waste heal when dead or already full.
        return playerHealth.CurrentHealth.Value > 0f &&
               playerHealth.CurrentHealth.Value < playerHealth.MaxHealth;
    }

    protected override void ExecuteAbility()
    {
        playerHealth.Heal(healAmount);
    }
}