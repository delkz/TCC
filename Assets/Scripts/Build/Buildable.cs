using UnityEngine;

public class Buildable : MonoBehaviour
{
    [SerializeField] private int buildCost = 10;
    [SerializeField] private bool canBeDestroyed = true;

    public int BuildCost => buildCost;
    public bool CanBeDestroyed => canBeDestroyed;
}
