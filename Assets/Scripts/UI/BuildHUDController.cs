using UnityEngine;
using TMPro;

public class BuildHUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI buildingText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GoldManager goldManager;

    private void Start()
    {
        if (goldManager != null)
        {
            goldManager.OnGoldChanged += UpdateMoney;
            UpdateMoney(goldManager.CurrentGold);
        }
    }

    private void OnDestroy()
    {
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
}
