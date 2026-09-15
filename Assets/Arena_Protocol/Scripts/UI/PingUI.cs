using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class PingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private float updateInterval = 0.5f;

    private UnityTransport transport;
    private float nextUpdateTime;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            transport =
                NetworkManager.Singleton.NetworkConfig.NetworkTransport
                as UnityTransport;
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextUpdateTime)
            return;

        nextUpdateTime =
            Time.unscaledTime + updateInterval;

        UpdatePing();
    }

    private void UpdatePing()
    {
        if (pingText == null ||
            transport == null ||
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            pingText.text = "0 ms";
            return;
        }

        ulong ping = transport.GetCurrentRtt(
            NetworkManager.ServerClientId
        );

        pingText.text = $"{ping} ms";
    }
}