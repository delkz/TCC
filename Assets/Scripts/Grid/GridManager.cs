using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject worldSprite;

    [System.Serializable]
    private class Cell
    {
        public int x;
        public int y;
        public bool isOccupied;
        public bool isBuildable;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;

            isOccupied = false;
            isBuildable = true;
        }
    }

    private Cell[,] grid;

    [System.Serializable]
    public class GridData
    {
        public int width;
        public int height;
        public CellData[] cells;
    }

    [System.Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public bool isOccupied;
        public bool isBuildable;
    }

    //public GridData ExportGridData() { }
    //public void LoadGridData(GridData data) { }
    public bool IsCellBuildable(int x, int y)
    {
        if (!isValidCell(x, y))
            return false;

        return !grid[x, y].isOccupied && grid[x, y].isBuildable;
    }

    public void SetCellOccupied(int x, int y, bool occupied)
    {
        if (!isValidCell(x, y))
            return;

        grid[x, y].isOccupied = occupied;
    }

    private void Awake()
    {
        createGrid();
    }

    
    private void createGrid()
    {
        grid = new Cell[width, height];

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y);
            }
        }

        Debug.Log($"Grid criado: {width} x {height}");
        centerCamera();
        resizeWorld();
    }

    private void centerCamera()
    {
        mainCamera.GetComponent<Transform>().position = new Vector3(width/2, height/2, -10);
    }

    private void resizeWorld()
    {
        Transform transform = worldSprite.GetComponent<Transform>();
        Vector3 scale = transform.localScale;

        scale.x = width;
        scale.y = height;


        transform.localScale = scale;
        worldSprite.GetComponent<Transform>().position = new Vector3(width / 2, height / 2, 0);
    }

    public bool isValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);

        return new Vector2Int(x, y);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellCenter = new Vector3(
                    x * cellSize + cellSize / 2f,
                    y * cellSize + cellSize / 2f,
                    0f
                );

                Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize);
            }
        }
    }

}
