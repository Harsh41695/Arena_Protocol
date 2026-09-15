using UnityEngine;

public class GameplayHUD : MonoBehaviour
{
    public static GameplayHUD Instance { get; private set; }

    [SerializeField] private GameObject hudContent;
    [SerializeField] private PlayerHealthUI healthUI;

    private void Awake()
    {
        Instance = this;

        if (hudContent != null)
            hudContent.SetActive(false);
    }

    public void InitializeLocalPlayer(PlayerHealth playerHealth)
    {
        healthUI?.Initialize(playerHealth);

        if (hudContent != null)
            hudContent.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}