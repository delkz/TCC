using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Grid/Grid Theme")]
public class GridTheme : ScriptableObject
{
    [Serializable]
    public class CellVisualOverride
    {
        public CellType type;
        public Sprite sprite;
        public Color color = Color.white;
        public int sortingOrder = 2;
    }

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

    [Header("Custom Cell Overrides")]
    public List<CellVisualOverride> customVisuals = new();

    public bool TryGetSprite(CellType type, out Sprite sprite, out int sortingOrder)
    {
        switch (type)
        {
            case CellType.Empty:
                sprite = emptySprite;
                sortingOrder = 0;
                return sprite != null;
            case CellType.Path:
                sprite = pathSprite;
                sortingOrder = 1;
                return sprite != null;
            case CellType.Blocked:
                sprite = blockedSprite;
                sortingOrder = 2;
                return sprite != null;
            case CellType.Spawn:
                sprite = spawnSprite;
                sortingOrder = 3;
                return sprite != null;
            case CellType.Goal:
                sprite = goalSprite;
                sortingOrder = 3;
                return sprite != null;
            default:
                return TryGetCustomSprite(type, out sprite, out sortingOrder);
        }
    }

    public bool TryGetColor(CellType type, out Color color)
    {
        switch (type)
        {
            case CellType.Empty:
                color = emptyColor;
                return true;
            case CellType.Path:
                color = pathColor;
                return true;
            case CellType.Spawn:
                color = spawnColor;
                return true;
            case CellType.Goal:
                color = goalColor;
                return true;
            case CellType.Blocked:
                color = blockedColor;
                return true;
            default:
                return TryGetCustomColor(type, out color);
        }
    }

    private bool TryGetCustomSprite(CellType type, out Sprite sprite, out int sortingOrder)
    {
        if (customVisuals != null)
        {
            for (int i = 0; i < customVisuals.Count; i++)
            {
                CellVisualOverride custom = customVisuals[i];
                if (custom != null && custom.type == type)
                {
                    sprite = custom.sprite;
                    sortingOrder = custom.sortingOrder;
                    return sprite != null;
                }
            }
        }

        sprite = null;
        sortingOrder = 2;
        return false;
    }

    private bool TryGetCustomColor(CellType type, out Color color)
    {
        if (customVisuals != null)
        {
            for (int i = 0; i < customVisuals.Count; i++)
            {
                CellVisualOverride custom = customVisuals[i];
                if (custom != null && custom.type == type)
                {
                    color = custom.color;
                    return true;
                }
            }
        }

        color = default;
        return false;
    }
}
