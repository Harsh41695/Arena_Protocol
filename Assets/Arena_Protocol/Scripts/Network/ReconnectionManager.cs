using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class ReconnectionManager : MonoBehaviour
{
    public static ReconnectionManager Instance { get; private set; }

    [Header("Player Spawn Points")]
    [SerializeField] private Transform hostSpawnPoint;
    [SerializeField] private Transform clientSpawnPoint;

    // NGO ClientId -> persistent PlayerId
    private readonly Dictionary<ulong, string> clientPlayerIds =
        new Dictionary<ulong, string>();

    // Persistent PlayerId -> saved gameplay state
    private readonly Dictionary<string, PlayerSessionState> savedStates =
        new Dictionary<string, PlayerSessionState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        NetworkManager.Singleton.ConnectionApprovalCallback =
            ApprovalCheck;

        NetworkManager.Singleton.OnClientDisconnectCallback +=
            HandleClientDisconnected;
    }

   

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string playerId =
            Encoding.UTF8.GetString(request.Payload);

        if (string.IsNullOrWhiteSpace(playerId))
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "Invalid Player ID.";
            response.Pending = false;

            return;
        }

        clientPlayerIds[request.ClientNetworkId] = playerId;

        response.Approved = true;
        response.CreatePlayerObject = true;

        // Assign different spawn points.
        if (playerId == "HostPlayer")
        {
            if (hostSpawnPoint != null)
            {
                response.Position =
                    hostSpawnPoint.position;

                response.Rotation =
                    hostSpawnPoint.rotation;
            }
        }
        else if (playerId == "ClientPlayer")
        {
            if (clientSpawnPoint != null)
            {
                response.Position =
                    clientSpawnPoint.position;

                response.Rotation =
                    clientSpawnPoint.rotation;
            }
        }

        response.Pending = false;

        Debug.Log(
            $"Approved Client {request.ClientNetworkId} " +
            $"as {playerId}"
        );
    }

  

    public void SavePlayerState(
        ulong clientId,
        PlayerSession playerSession)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer ||
            playerSession == null)
        {
            return;
        }

        if (!clientPlayerIds.TryGetValue(
                clientId,
                out string playerId))
        {
            Debug.LogWarning(
                $"Could not save state for Client {clientId}. " +
                "Player ID not found."
            );

            return;
        }

        PlayerSessionState state =
            playerSession.CaptureState(playerId);

        savedStates[playerId] = state;

        Debug.Log(
            $"Saved state for {playerId} | " +
            $"Health: {state.Health} | " +
            $"Dash End: {state.DashCooldownEndTime:F2} | " +
            $"Projectile End: {state.ProjectileCooldownEndTime:F2} | " +
            $"Heal End: {state.HealCooldownEndTime:F2}"
        );
    }

   

    public void RestorePlayerState(
        ulong clientId,
        PlayerSession playerSession)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer ||
            playerSession == null)
        {
            return;
        }

        if (!clientPlayerIds.TryGetValue(
                clientId,
                out string playerId))
        {
            Debug.LogWarning(
                $"Could not restore Client {clientId}. " +
                "Player ID not found."
            );

            return;
        }

        if (!savedStates.TryGetValue(
                playerId,
                out PlayerSessionState state))
        {
            Debug.Log(
                $"No saved state for {playerId}. Starting fresh."
            );

            return;
        }

        playerSession.RestoreState(state);

        Debug.Log(
            $"Restored state for {playerId} | " +
            $"Health: {state.Health}"
        );
    }


    private void HandleClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (!clientPlayerIds.TryGetValue(
                clientId,
                out string playerId))
        {
            return;
        }

        Debug.Log(
            $"Player disconnected: {playerId} | " +
            $"ClientId: {clientId}"
        );

        // PlayerSession.OnNetworkDespawn() captures the state
        // before this mapping is removed.
        clientPlayerIds.Remove(clientId);
    }

   

    public string GetPlayerId(ulong clientId)
    {
        if (clientPlayerIds.TryGetValue(
                clientId,
                out string playerId))
        {
            return playerId;
        }

        return null;
    }


    public Vector3 GetSpawnPosition(ulong clientId)
    {
        if (!clientPlayerIds.TryGetValue(
                clientId,
                out string playerId))
        {
            Debug.LogWarning(
                $"No Player ID found for ClientId {clientId}."
            );

            return Vector3.zero;
        }

        if (playerId == "HostPlayer")
        {
            if (hostSpawnPoint != null)
                return hostSpawnPoint.position;
        }
        else if (playerId == "ClientPlayer")
        {
            if (clientSpawnPoint != null)
                return clientSpawnPoint.position;
        }

        Debug.LogWarning(
            $"Spawn point not found for {playerId}."
        );

        return Vector3.zero;
    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -=
                HandleClientDisconnected;

            if (NetworkManager.Singleton.ConnectionApprovalCallback ==
                ApprovalCheck)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback =
                    null;
            }
        }

        if (Instance == this)
            Instance = null;
    }
}