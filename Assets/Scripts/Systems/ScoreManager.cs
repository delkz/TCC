using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; }
    public int TargetScore { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action OnLevelCompleted;

    private bool levelCompletedEmitted;

    private void Awake()
    {
        if (GameSession.Instance == null || GameSession.Instance.SelectedLevel == null)
        {
            Debug.LogWarning("Gameplay iniciada sem LevelData (modo debug).");
            return;
        }

        LevelData level = GameSession.Instance.SelectedLevel;
        if (level == null)
        {
            return;
        }

        TargetScore = Mathf.Max(0, level.scoreTarget);
        SetInitialScore(0);

        Debug.Log($"Score inicial: {CurrentScore} | Meta: {TargetScore}");
    }

    public void SetInitialScore(int amount)
    {
        CurrentScore = Mathf.Max(0, amount);
        OnScoreChanged?.Invoke(CurrentScore);
        CheckLevelCompletion();
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);

        Debug.Log($"Score ganho: {amount} | Atual: {CurrentScore} | Meta: {TargetScore}");
        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (levelCompletedEmitted)
        {
            return;
        }

        if (CurrentScore < TargetScore)
        {
            return;
        }

        levelCompletedEmitted = true;
        Debug.Log($"Meta de score atingida! Score: {CurrentScore} / {TargetScore}");
        GameAudioEvents.RaiseLevelCompleted();
        OnLevelCompleted?.Invoke();
    }
}
