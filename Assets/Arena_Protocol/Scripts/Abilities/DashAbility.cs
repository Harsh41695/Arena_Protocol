using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DashAbility : AbilityBase
{
    [Header("Dash")]
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashDuration = 0.15f;

    private CharacterController characterController;
    private Coroutine dashCoroutine;

    protected override void Awake()
    {
        base.Awake();

        characterController =
            GetComponent<CharacterController>();
    }

    protected override bool CanUseAbility()
    {
        return characterController != null;
    }

    // Called on Server after cooldown validation
    protected override void ExecuteAbility()
    {
        StartDashRpc();
    }

    // Execute movement only on the owning player
    [Rpc(SendTo.Owner)]
    private void StartDashRpc()
    {
        if (dashCoroutine != null)
            StopCoroutine(dashCoroutine);

        dashCoroutine = StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 dashDirection = transform.forward.normalized;

        Vector3 targetPosition =
            startPosition + dashDirection * dashDistance;

        float elapsed = 0f;
        Vector3 previousPosition = startPosition;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / dashDuration);

            Vector3 desiredPosition =
                Vector3.Lerp(startPosition, targetPosition, t);

            Vector3 movement =
                desiredPosition - previousPosition;

            characterController.Move(movement);

            previousPosition = desiredPosition;

            yield return null;
        }

        dashCoroutine = null;
    }
}