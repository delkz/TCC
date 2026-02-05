using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBuildingData",
    menuName = "TowerDefense/Building Data"
)]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName;
    public GameObject prefab;
    public Sprite icon;

    [Header("Economy")]
    public int buildCost;
    [Range(0f, 1f)]
    public float refundPercent = 0.66f;

    [Header("Rules")]
    public bool canBeDestroyed = true;

    [Header("Tower Stats")]
    public int damage;
    public float attackCooldown;
    public float range;

    [Header("Effects")]
    public float slowAmount;
    public float slowDuration;
    public float knockback;
}
