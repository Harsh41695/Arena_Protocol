using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;

    [Header("Text")]
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text readyText;

    private AbilityBase ability;

    public void Initialize(AbilityBase targetAbility)
    {
        ability = targetAbility;
    }

    private void Update()
    {
        if (ability == null)
            return;

        float remaining = ability.RemainingCooldown;
        float duration = ability.CooldownDuration;

        bool isReady = remaining <= 0f;

        if (isReady)
        {
            if (cooldownFill != null)
                cooldownFill.fillAmount = 0f;

            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);

            if (readyText != null)
                readyText.gameObject.SetActive(true);
        }
        else
        {
            if (cooldownFill != null)
                cooldownFill.fillAmount = remaining / duration;

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = remaining.ToString("0.0");
            }

            if (readyText != null)
                readyText.gameObject.SetActive(false);
        }
    }
}