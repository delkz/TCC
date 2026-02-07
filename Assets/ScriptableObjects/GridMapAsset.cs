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

    public void SetCell(int x, int y, CellType type)
    {
        cells[x + y * width] = type;
    }

    public void Resize()
    {
        cells = new CellType[width * height];
    }
}
