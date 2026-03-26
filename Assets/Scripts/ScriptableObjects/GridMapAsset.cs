using UnityEngine;

[CreateAssetMenu(menuName = "Grid/Grid Map")]
public class GridMapAsset : ScriptableObject
{
    public int width;
    public int height;

    public GridTheme theme;

    [HideInInspector]
    public CellType[] cells;

    public CellType GetCell(int x, int y)
    {
        return cells[x + y * width];
    }

    public void SetCell(int x, int y, CellType newType)
    {
        if (!IsValid(x, y))
            return;

        // garante 1 Spawn
        if (newType == CellType.Spawn)
            ClearAll(CellType.Spawn);

        // garante 1 Goal
        if (newType == CellType.Goal)
            ClearAll(CellType.Goal);

        cells[x + y * width] = newType;
    }

    private void ClearAll(CellType type)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == type)
                cells[i] = CellType.Empty;
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void Resize()
    {
        cells = new CellType[width * height];
    }
}
