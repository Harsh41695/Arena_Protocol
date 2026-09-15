using System;
using Unity.Netcode;
using UnityEngine;

public class EnergyOrb : NetworkBehaviour
{
    [SerializeField] private int scoreValue = 1;

    public event Action<EnergyOrb> OnCollected;

    private bool collected;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            collected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || collected)
            return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null || player.CurrentHealth.Value <= 0f)
            return;

        collected = true;

        TeamScoreManager.Instance?.AddScore(scoreValue);

        OnCollected?.Invoke(this);

        NetworkObject.Despawn();
    }
}