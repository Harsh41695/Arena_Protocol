using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private PlayerHealth playerHealth;

    public void Initialize(PlayerHealth health)
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        playerHealth = health;

        if (playerHealth == null)
            return;

        playerHealth.OnHealthChanged += HandleHealthChanged;

        UpdateHealth(
            playerHealth.CurrentHealth.Value,
            playerHealth.MaxHealth
        );
    }

    private void HandleHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        UpdateHealth(currentHealth, maxHealth);
    }

    private void UpdateHealth(
        float currentHealth,
        float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{Mathf.CeilToInt(currentHealth)} / " +
                $"{Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }
}