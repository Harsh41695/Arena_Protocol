using TMPro;
using UnityEngine;

public class TeamScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private TeamScoreManager scoreManager;

    private void Start()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        scoreManager = TeamScoreManager.Instance;

        if (scoreManager == null)
        {
            Debug.LogWarning("TeamScoreManager not found.");
            return;
        }

        scoreManager.TeamScore.OnValueChanged += HandleScoreChanged;

        UpdateScore(scoreManager.TeamScore.Value);
    }

    private void HandleScoreChanged(int previousValue, int newValue)
    {
        UpdateScore(newValue);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"TEAM SCORE: {score}";
    }

    private void OnDestroy()
    {
        if (scoreManager != null)
        {
            scoreManager.TeamScore.OnValueChanged -= HandleScoreChanged;
        }
    }
}