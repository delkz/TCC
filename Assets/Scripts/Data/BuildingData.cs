using UnityEngine;

[System.Serializable]
public struct BuildingLevelStats
{
    [Header("Economy")]
    public int upgradeCost;

    [Header("Visual")]
    public Sprite levelSprite;

    [Header("Tower Stats")]
    public int damage;
    public float attackCooldown;
    public float range;

    [Header("Effects")]
    public float slowAmount;
    public float slowDuration;
    public float knockback;
}

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

    [Header("Upgrades")]
    public BuildingLevelStats level2;
    public bool hasLevel3;
    public BuildingLevelStats level3;

    public int MaxLevel => hasLevel3 ? 3 : 2;

    public bool TryGetLevelStats(int level, out BuildingLevelStats stats)
    {
        if (level == 2)
        {
            stats = level2;
            return true;
        }

        if (level == 3 && hasLevel3)
        {
            stats = level3;
            return true;
        }

        stats = default;
        return false;
    }

    public int GetUpgradeCostForLevel(int level)
    {
        if (!TryGetLevelStats(level, out BuildingLevelStats stats))
        {
            return 0;
        }

        return Mathf.Max(0, stats.upgradeCost);
    }

    public Sprite GetSpriteForLevel(int level)
    {
        if (level <= 1)
        {
            return icon;
        }

        if (TryGetLevelStats(level, out BuildingLevelStats stats) && stats.levelSprite != null)
        {
            return stats.levelSprite;
        }

        return icon;
    }
}
