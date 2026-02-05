using UnityEngine;
using TMPro;

public class BuildHUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI buildingText;
    [SerializeField] private TextMeshProUGUI moneyText;

    [SerializeField] private GoldManager goldManager;
    [Header("References")]
    [SerializeField] private Nexus nexus;
    [SerializeField] private TextMeshProUGUI nexusLifeText;
    private void Start()
    {
        if (nexus == null)
        {
            nexus = FindObjectOfType<Nexus>();
        }

        if (nexus != null)
        {
            nexus.OnHealthChanged += UpdateNexusHealth;
            nexus.OnNexusDestroyed += HandleNexusDestroyed;

            UpdateNexusHealth(nexus.currentHealth);
        }

        if (goldManager != null)
        {
            goldManager.OnGoldChanged += UpdateMoney;
            UpdateMoney(goldManager.CurrentGold);
        }
    }

    private void OnDestroy()
    {
        if (nexus != null)
        {
            nexus.OnHealthChanged -= UpdateNexusHealth;
            nexus.OnNexusDestroyed -= HandleNexusDestroyed;
        }
        if (goldManager != null)
        {
            goldManager.OnGoldChanged -= UpdateMoney;
        }
    }

    public void UpdateMode(BuildMode mode)
    {
        modeText.text = $"Mode: {mode}";
    }

    public void UpdateBuilding(string buildingName)
    {
        buildingText.text = $"Building: {buildingName}";
    }

    private void UpdateMoney(int money)
    {
        moneyText.text = $"${money}";
    }

    private void UpdateNexusHealth(int health)
    {
        if (nexusLifeText != null)
        {
            nexusLifeText.text = $"Nexus HP: {health}";
        }
    }

    private void HandleNexusDestroyed()
    {
        Debug.Log("UIManager recebeu evento: Nexus destruído");

        // por enquanto só feedback
        // depois vira tela de Game Over
    }
}
