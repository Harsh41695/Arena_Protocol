using Unity.Netcode;
using UnityEngine;

public class TeamScoreManager : NetworkBehaviour
{
    public static TeamScoreManager Instance { get; private set; }

    public NetworkVariable<int> TeamScore =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            TeamScore.Value = 0;
    }

    public void AddScore(int amount)
    {
        if (!IsServer || amount <= 0)
            return;

        TeamScore.Value += amount;
    }

    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        base.OnDestroy();
    }
}