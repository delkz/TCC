using UnityEngine;

[CreateAssetMenu(menuName = "Grid/Grid Theme")]
public class GridTheme : ScriptableObject
{
    [Header("Sprites")]
    public Sprite emptySprite;
    public Sprite pathSprite;
    public Sprite spawnSprite;
    public Sprite goalSprite;
    public Sprite blockedSprite;

    [Header("Path Auto Tiles")]
    public Sprite pathStraight;
    public Sprite pathCorner;
    public Sprite pathT;
    public Sprite pathCross;
    public Sprite pathEnd;

    [Header("Fallback Colors")]
    public Color emptyColor = Color.gray;
    public Color pathColor = Color.yellow;
    public Color spawnColor = Color.green;
    public Color goalColor = Color.red;
    public Color blockedColor = Color.black;
}
