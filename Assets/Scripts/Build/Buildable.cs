using UnityEngine;
using System;

public class Buildable : MonoBehaviour
{
    [SerializeField] private BuildingData data;

    private int currentLevel = 1;
    private int totalSpentGold;

    private int damage;
    private float attackCooldown;
    private float range;
    private float slowAmount;
    private float slowDuration;
    private float knockback;

    public BuildingData Data => data;
    public bool CanBeDestroyed => data.canBeDestroyed;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => data != null ? data.MaxLevel : 1;
    public bool CanUpgrade => currentLevel < MaxLevel;

    public int Damage => damage;
    public float AttackCooldown => attackCooldown;
    public float Range => range;
    public float SlowAmount => slowAmount;
    public float SlowDuration => slowDuration;
    public float Knockback => knockback;

    public event Action<int> OnLevelChanged;

    private void Awake()
    {
        InitializeRuntimeData();
    }

    private void InitializeRuntimeData()
    {
        if (data == null)
        {
            return;
        }

        currentLevel = 1;
        totalSpentGold = Mathf.Max(0, data.buildCost);
        ApplyStatsForLevel(currentLevel);
        ApplySpriteForCurrentLevel();
        OnLevelChanged?.Invoke(currentLevel);
    }

    public int GetUpgradeCost()
    {
        if (!CanUpgrade || data == null)
        {
            return 0;
        }

        int nextLevel = currentLevel + 1;
        return data.GetUpgradeCostForLevel(nextLevel);
    }

    public bool TryUpgrade(GoldManager goldManager)
    {
        if (!CanUpgrade || goldManager == null)
        {
            return false;
        }

        int cost = GetUpgradeCost();
        if (!goldManager.Spend(cost))
        {
            return false;
        }

        currentLevel += 1;
        totalSpentGold += cost;

        ApplyStatsForLevel(currentLevel);
        ApplySpriteForCurrentLevel();
        OnLevelChanged?.Invoke(currentLevel);

        return true;
    }

    private void ApplyStatsForLevel(int level)
    {
        if (data == null)
        {
            return;
        }

        damage = data.damage;
        attackCooldown = data.attackCooldown;
        range = data.range;
        slowAmount = data.slowAmount;
        slowDuration = data.slowDuration;
        knockback = data.knockback;

        if (level <= 1)
        {
            return;
        }

        if (data.TryGetLevelStats(level, out BuildingLevelStats levelStats))
        {
            damage = levelStats.damage;
            attackCooldown = levelStats.attackCooldown;
            range = levelStats.range;
            slowAmount = levelStats.slowAmount;
            slowDuration = levelStats.slowDuration;
            knockback = levelStats.knockback;
        }
    }

    private void ApplySpriteForCurrentLevel()
    {
        if (data == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite levelSprite = data.GetSpriteForLevel(currentLevel);
        if (levelSprite != null)
        {
            spriteRenderer.sprite = levelSprite;
        }
    }

    public int GetRefundValue()
    {
        return Mathf.FloorToInt(totalSpentGold * data.refundPercent);
    }
}
