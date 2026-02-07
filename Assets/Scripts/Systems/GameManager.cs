using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<bool> OnPauseChanged;
    public event Action<float> OnSpeedChanged;

    public enum GameState
    {
        Running,
        Paused
    }

    public GameState CurrentState { get; private set; } = GameState.Running;

    // ================= SPEED =================

    [SerializeField]
    private float[] speedLevels = { 1f, 2f, 5f };

    private int currentSpeedIndex = 0;
    private float gameSpeed => speedLevels[currentSpeedIndex];

    // ================= UNITY =================

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
        OnSpeedChanged?.Invoke(gameSpeed);
    }

    // ================= PAUSE =================

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

    // ================= SPEED =================

    public void ToggleSpeed()
    {
        currentSpeedIndex++;

        if (currentSpeedIndex >= speedLevels.Length)
            currentSpeedIndex = 0;

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

    // ================= UTILITÁRIOS =================
    public void ResetTime()
    {
        CurrentState = GameState.Running;
        Time.timeScale = 1f;
        OnPauseChanged?.Invoke(false);
    }

    public void ReturnToMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetTime();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }


}
