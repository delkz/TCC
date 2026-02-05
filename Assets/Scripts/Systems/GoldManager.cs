using UnityEngine;
using System;

public class GoldManager : MonoBehaviour
{
    [SerializeField] private int startingGold = 50;

    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        CurrentGold = startingGold;
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
