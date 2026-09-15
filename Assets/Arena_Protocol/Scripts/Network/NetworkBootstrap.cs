using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject connectionUI;

    private const string HostPlayerId = "HostPlayer";
    private const string ClientPlayerId = "ClientPlayer";

    public void StartHost()
    {
        SetConnectionData(HostPlayerId);

        if (NetworkManager.Singleton.StartHost())
            HideConnectionUI();
    }

    public void StartClient()
    {
        SetConnectionData(ClientPlayerId);

        if (NetworkManager.Singleton.StartClient())
            HideConnectionUI();
    }

    private void SetConnectionData(string playerId)
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            Encoding.UTF8.GetBytes(playerId);
    }

    private void HideConnectionUI()
    {
        if (connectionUI != null)
            connectionUI.SetActive(false);
    }
}