using Unity.Netcode;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject connectionUI;

    public void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            HideConnectionUI();
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            HideConnectionUI();
        }
    }

    private void HideConnectionUI()
    {
        if (connectionUI != null)
        {
            connectionUI.SetActive(false);
        }
    }
}