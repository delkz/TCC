using UnityEngine;
using System;

public class GoldManager : MonoBehaviour
{
    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;
    public void SetInitialGold(int amount)
    {
        CurrentGold = amount;
        OnGoldChanged?.Invoke(CurrentGold);
    }
    private void Awake()
    {
        if (GameSession.Instance == null || GameSession.Instance.SelectedLevel == null)
        {
            Debug.LogWarning("Gameplay iniciada sem LevelData (modo debug).");
            return;
        }

        LevelData level = GameSession.Instance.SelectedLevel;

        if (level == null)
            return;

        SetInitialGold(level.startingGold);
        OnGoldChanged?.Invoke(CurrentGold);
        Debug.Log($"Gold inicial: {CurrentGold}");
    }

    public bool CanAfford(int amount)
    {
        return CurrentGold >= amount;
    }

    public bool Spend(int amount)
    {
        if (!CanAfford(amount))
            return false;

        CurrentGold -= amount;
        OnGoldChanged?.Invoke(CurrentGold);

        Debug.Log($"Gold gasto: {amount} | Atual: {CurrentGold}");
        return true;
    }

    public void Add(int amount)
    {
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);

        Debug.Log($"Gold ganho: {amount} | Atual: {CurrentGold}");
    }
}
