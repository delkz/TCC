using UnityEngine;
using TMPro;

public class BuildHUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI buildingText;

    public void UpdateMode(BuildMode mode)
    {
        modeText.text = $"Mode: {mode}";
    }

    public void UpdateBuilding(string buildingName)
    {
        buildingText.text = $"Building: {buildingName}";
    }
}
