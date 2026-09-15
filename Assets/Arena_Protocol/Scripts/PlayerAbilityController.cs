using Unity.Netcode;
using UnityEngine;

public class PlayerAbilityController : NetworkBehaviour
{
    [Header("Abilities")]
    [SerializeField] private DashAbility dashAbility;
    [SerializeField] private ProjectileAbility projectileAbility;
    [SerializeField] private HealAbility healAbility;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (!Application.isFocused)
            return;

        HandleAbilityInput();
    }

    private void HandleAbilityInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dashAbility?.TryUseAbility();
        }

        if (Input.GetMouseButtonDown(0)|| Input.GetKeyDown(KeyCode.X))
        {
            projectileAbility?.TryUseAbility();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            healAbility?.TryUseAbility();
        }
    }
}