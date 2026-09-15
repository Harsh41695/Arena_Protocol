using UnityEngine;

public class GameplayHUD : MonoBehaviour
{
    public static GameplayHUD Instance { get; private set; }

    [SerializeField] private GameObject hudContent;

    [Header("Health")]
    [SerializeField] private PlayerHealthUI healthUI;

    [Header("Abilities")]
    [SerializeField] private AbilityCooldownUI dashCooldownUI;
    [SerializeField] private AbilityCooldownUI projectileCooldownUI;
    [SerializeField] private AbilityCooldownUI healCooldownUI;

    private void Awake()
    {
        Instance = this;

        if (hudContent != null)
            hudContent.SetActive(false);
    }

    public void InitializeLocalPlayer(
        PlayerHealth playerHealth,
        DashAbility dash,
        ProjectileAbility projectile,
        HealAbility heal)
    {
        healthUI?.Initialize(playerHealth);

        dashCooldownUI?.Initialize(dash);
        projectileCooldownUI?.Initialize(projectile);
        healCooldownUI?.Initialize(heal);

        if (hudContent != null)
            hudContent.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}