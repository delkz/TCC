using UnityEngine;
using TMPro;

public class GameHUDController : MonoBehaviour
{
    [Header("Build Info")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI buildingText;

    [Header("Economy")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GoldManager goldManager;

    [Header("Nexus")]
    [SerializeField] private Nexus nexus;
    [SerializeField] private TextMeshProUGUI nexusLifeText;

    [Header("Pause")]
    [SerializeField] private GameObject pauseOverlay;

    [Header("Speed")]
    [SerializeField] private TextMeshProUGUI speedText;

    private void Start()
    {
        BindGold();
        BindNexus();
        BindGameManager();
    }

    private void OnDestroy()
    {
        UnbindGold();
        UnbindNexus();
        UnbindGameManager();
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

    private void BindGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnPauseChanged += HandlePause;
        GameManager.Instance.OnSpeedChanged += HandleSpeed;

        // estado inicial
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

    // =======================
    // Event Handlers
    // =======================

    private void UpdateMoney(int money)
    {
        moneyText.text = $"${money}";
    }

    private void UpdateNexusHealth(int health)
    {
        if (nexusLifeText != null)
            nexusLifeText.text = $"Nexus HP: {health}";
    }

    private void HandleNexusDestroyed()
    {
        UILogger.Log("HUD recebeu evento: Nexus destruído");
        // futuramente: tela de Game Over
    }

    private void HandlePause(bool isPaused)
    {
        if (pauseOverlay != null)
            pauseOverlay.SetActive(isPaused);
    }

    private void HandleSpeed(float speed)
    {
        if (speedText != null)
            speedText.text = $"{speed}x";
    }
}
