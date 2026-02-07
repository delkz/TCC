using UnityEngine;

public class GridCell
{
    public Vector2Int position;
    public CellType type;
    public bool isBuildable;
    public GameObject occupant;

    public GridCell(int x, int y, CellType type)
    {
        position = new Vector2Int(x, y);
        this.type = type;
        occupant = null;

        // regra base
        isBuildable = type == CellType.Empty;
    }

    public bool IsOccupied => occupant != null;
    public bool IsPath => type == CellType.Path;
    public bool IsSpawn => type == CellType.Spawn;
    public bool IsGoal => type == CellType.Goal;
}
