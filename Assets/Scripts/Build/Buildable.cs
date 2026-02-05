using UnityEngine;

public class Buildable : MonoBehaviour
{
    [SerializeField] private BuildingData data;

    public BuildingData Data => data;
    public bool CanBeDestroyed => data.canBeDestroyed;

    public int GetRefundValue()
    {
        return Mathf.FloorToInt(data.buildCost * data.refundPercent);
    }
}
