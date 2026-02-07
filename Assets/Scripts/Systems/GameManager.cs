using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event System.Action<bool> OnPauseChanged;
    public event System.Action<float> OnSpeedChanged;

    public enum GameState
    {
        Running,
        Paused
    }

    public GameState CurrentState { get; private set; } = GameState.Running;

    private float gameSpeed = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyTimeScale();
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Running)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }
        else
        {
            CurrentState = GameState.Running;
            ApplyTimeScale();
            OnPauseChanged?.Invoke(false);
        }
    }

    public void ToggleSpeed()
    {
        gameSpeed = gameSpeed == 1f ? 2f : 1f;

        if (CurrentState == GameState.Running)
            ApplyTimeScale();

        OnSpeedChanged?.Invoke(gameSpeed);
    }
    private void ApplyTimeScale()
    {
        Time.timeScale = gameSpeed;
    }

    public float GetGameSpeed()
    {
        return gameSpeed;
    }
}
