using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameHUDController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float endScreenDuration = 5f;

    // UI Elements
    private Label modeText;
    private Label buildingText;
    private Label moneyText;
    private Label nexusLifeText;
    private Label speedText;
    private Label scoreText;

    private Label waveText;

    private VisualElement pauseOverlay;
    private VisualElement gameOverOverlay;
    private VisualElement levelCompletedOverlay;

    [Header("Economy")]
    [SerializeField] private GoldManager goldManager;

    [Header("Nexus")]
    [SerializeField] private Nexus nexus;
    [Header("Wave Manager")]
    [SerializeField] private WaveManager waveManager;

    private ScoreManager scoreManager;
    private bool endSequenceStarted;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        modeText = root.Q<Label>("ModeText");
        buildingText = root.Q<Label>("BuildingText");

        moneyText = root.Q<Label>("GoldLabel");
        nexusLifeText = root.Q<Label>("HealthLabel");
        scoreText = root.Q<Label>("ScoreLabel");

        speedText = root.Q<Label>("SpeedText");
        waveText = root.Q<Label>("WaveLabel");

        pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        gameOverOverlay = root.Q<VisualElement>("GameOverOverlay");
        levelCompletedOverlay = root.Q<VisualElement>("LevelCompletedOverlay");

        scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void Start()
    {
        BindGold();
        BindNexus();
        BindGameManager();
        BindWave();
        BindScore();
    }

    private void OnDestroy()
    {
        UnbindGold();
        UnbindNexus();
        UnbindGameManager();
        UnbindWave();
        UnbindScore();
    }

    // =======================
    // Bindings
    // =======================

    private void BindGold()
    {
        if (goldManager == null) return;

        goldManager.OnGoldChanged += UpdateMoney;
        UpdateMoney(goldManager.CurrentGold);
    }

    private void UnbindGold()
    {
        if (goldManager == null) return;
        goldManager.OnGoldChanged -= UpdateMoney;
    }

    private void BindWave()
    {
        if (waveManager == null) return;

        waveManager.waveStarted += HandleWaveStarted;

        UpdateWave(waveManager.currentWave);
    }
    private void UnbindWave()
    {
        if (waveManager == null) return;

        waveManager.waveStarted -= HandleWaveStarted;
    }
    private void BindNexus()
    {
        if (nexus == null)
            nexus = FindObjectOfType<Nexus>();

        if (nexus == null) return;

        nexus.OnHealthChanged += UpdateNexusHealth;
        nexus.OnNexusDestroyed += HandleNexusDestroyed;

        UpdateNexusHealth(nexus.currentHealth);
    }

    private void UnbindNexus()
    {
        if (nexus == null) return;

        nexus.OnHealthChanged -= UpdateNexusHealth;
        nexus.OnNexusDestroyed -= HandleNexusDestroyed;
    }

    private void BindScore()
    {
        if (scoreManager == null) return;

        scoreManager.OnScoreChanged += UpdateScore;
        scoreManager.OnLevelCompleted += HandleLevelCompleted;
        UpdateScore(scoreManager.CurrentScore);
    }

    private void UnbindScore()
    {
        if (scoreManager == null) return;

        scoreManager.OnScoreChanged -= UpdateScore;
        scoreManager.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void BindGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnPauseChanged += HandlePause;
        GameManager.Instance.OnSpeedChanged += HandleSpeed;

        HandleSpeed(GameManager.Instance.GetGameSpeed());
    }

    private void UnbindGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnPauseChanged -= HandlePause;
        GameManager.Instance.OnSpeedChanged -= HandleSpeed;
    }

    // =======================
    // Public UI API
    // =======================

    public void UpdateMode(BuildMode mode)
    {
        if (modeText != null)
            modeText.text = $"Mode: {mode}";
    }

    public void UpdateBuilding(string buildingName, int cost = 0)
    {
        if (buildingText != null)
            buildingText.text = $"Building: {buildingName} (Cost: {cost})";
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"{wave}";
    }

    // =======================
    // Event Handlers
    // =======================

    private void UpdateMoney(int money)
    {
        if (moneyText != null)
            moneyText.text = $"${money}";
    }

    private void UpdateNexusHealth(int health)
    {
        if (nexusLifeText != null)
            nexusLifeText.text = $"{health}";
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score}";
    }

    private void HandleNexusDestroyed()
    {
        UILogger.Log("HUD recebeu evento: Nexus destruído");
        HandleGameOver();
    }

    private void HandleLevelCompleted()
    {
        UILogger.Log("HUD recebeu evento: Level completo");
        ShowEndOverlay(levelCompletedOverlay);
        StartEndSequence();
    }

    private void HandleWaveStarted(int wave)
    {
        UpdateWave(wave);
    }
    private void HandlePause(bool isPaused)
    {
        if (pauseOverlay != null)
            pauseOverlay.style.display = isPaused ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void HandleGameOver()
    {
        ShowEndOverlay(gameOverOverlay);
        StartEndSequence();
    }

    private void ShowEndOverlay(VisualElement overlay)
    {
        if (overlay != null)
        {
            overlay.style.display = DisplayStyle.Flex;
        }
    }

    private void StartEndSequence()
    {
        if (endSequenceStarted)
        {
            return;
        }

        endSequenceStarted = true;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Running)
        {
            GameManager.Instance.TogglePause();
        }

        StartCoroutine(BackToMenuSequence());
    }

    private IEnumerator BackToMenuSequence()
    {
        yield return new WaitForSecondsRealtime(endScreenDuration);
        SceneManager.LoadScene("Menu");
    }

    private void HandleSpeed(float speed)
    {
        if (speedText != null)
            speedText.text = $"{speed}x";
    }
}