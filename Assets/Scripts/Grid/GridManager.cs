using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public event Action OnGridChanged;

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private GameObject nexusPrefab;
    [SerializeField] private GameObject enemySpawnPrefab;
    [SerializeField] private GridMapAsset mapAsset;

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    // ===================== STATE =====================

    private int width;
    private int height;

    private GridCell[,] grid;
    private GameObject[,] tileVisuals;

    private Vector2Int spawnCell;
    private Vector2Int goalCell;

    // ===================== UNITY =====================

    private void Awake()
    {
        LoadMap(mapAsset);
    }

    // ===================== MAP LOADING =====================

    public void LoadMap(GridMapAsset map)
    {
        if (map == null)
        {
            Debug.LogError("GridManager: MapAsset não atribuído.");
            return;
        }

        width = map.width;
        height = map.height;

        CreateGridFromMap(map);
        GenerateGridVisual();
        SpawnMapEntities();
        CenterCamera();

        DebugPath();
    }

    private void CreateGridFromMap(GridMapAsset map)
    {
        grid = new GridCell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CellType type = map.GetCell(x, y);
                grid[x, y] = new GridCell(x, y, type);

                if (type == CellType.Spawn)
                    spawnCell = new Vector2Int(x, y);

                if (type == CellType.Goal)
                    goalCell = new Vector2Int(x, y);
            }
        }
    }

    // ===================== VISUAL GRID =====================

    private void GenerateGridVisual()
    {
        tileVisuals = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GridCell cell = grid[x, y];

                GameObject tile = Instantiate(
                    gridTilePrefab,
                    GridToWorldCenter(new Vector2Int(x, y)),
                    Quaternion.identity,
                    transform
                );

                tile.name = $"Tile_{x}_{y}";

                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                ApplyTheme(sr, cell.type);

                tileVisuals[x, y] = tile;
            }
        }
    }

    private void ApplyTheme(SpriteRenderer sr, CellType type)
    {
        GridTheme theme = mapAsset.theme;

        if (theme == null)
        {
            sr.color = Color.magenta;
            return;
        }

        Sprite sprite = type switch
        {
            CellType.Path => theme.pathSprite,
            CellType.Spawn => theme.spawnSprite,
            CellType.Goal => theme.goalSprite,
            CellType.Blocked => theme.blockedSprite,
            _ => theme.emptySprite
        };

        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.color = Color.white;
        }
        else
        {
            // fallback visual
            sr.sprite = null;
            sr.color = type switch
            {
                CellType.Path => theme.pathColor,
                CellType.Spawn => theme.spawnColor,
                CellType.Goal => theme.goalColor,
                CellType.Blocked => theme.blockedColor,
                _ => theme.emptyColor
            };
        }
    }


    // ===================== MAP ENTITIES =====================

    private void SpawnMapEntities()
    {
        SpawnEnemySpawnPoint();
        SpawnNexus();
    }

    private void SpawnEnemySpawnPoint()
    {
        GameObject spawnGO = Instantiate(
            enemySpawnPrefab,
            GridToWorldCenter(spawnCell),
            Quaternion.identity
        );

        SetCellOccupant(spawnCell, spawnGO);
    }

    private void SpawnNexus()
    {
        GameObject nexus = Instantiate(
            nexusPrefab,
            GridToWorldCenter(goalCell),
            Quaternion.identity
        );

        SetCellOccupant(goalCell, nexus);
    }

    // ===================== GRID API =====================

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool IsCellBuildable(int x, int y)
    {
        if (!IsValidCell(x, y))
            return false;

        GridCell cell = grid[x, y];
        return !cell.IsOccupied && cell.isBuildable;
    }

    public void SetCellOccupant(Vector2Int pos, GameObject occupant)
    {
        if (!IsValidCell(pos.x, pos.y))
            return;

        grid[pos.x, pos.y].occupant = occupant;
        OnGridChanged?.Invoke();
    }

    public GameObject GetCellOccupant(Vector2Int pos)
    {
        if (!IsValidCell(pos.x, pos.y))
            return null;

        return grid[pos.x, pos.y].occupant;
    }

    // ===================== PATH (LINEAR, MAP-DEFINED) =====================

    public List<Vector2Int> BuildPath()
    {
        List<Vector2Int> path = new();
        HashSet<Vector2Int> visited = new();

        Vector2Int current = spawnCell;
        path.Add(current);
        visited.Add(current);

        while (current != goalCell)
        {
            Vector2Int next = GetNextPathCell(current, visited);

            if (next == current)
            {
                Debug.LogError("GridManager: Caminho inválido no mapa.");
                break;
            }

            current = next;
            path.Add(current);
            visited.Add(current);
        }

        return path;
    }

    private Vector2Int GetNextPathCell(Vector2Int current, HashSet<Vector2Int> visited)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            Vector2Int next = current + dir;

            if (!IsValidCell(next.x, next.y))
                continue;

            if (visited.Contains(next))
                continue;

            if (grid[next.x, next.y].IsPath || grid[next.x, next.y].IsGoal)
                return next;
        }

        return current;
    }

    private void DebugPath()
    {
        foreach (var cell in BuildPath())
            Debug.Log($"Path cell: {cell}");
    }

    // ===================== COORDINATES =====================

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorldCenter(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize + cellSize / 2f,
            gridPos.y * cellSize + cellSize / 2f,
            0f
        );
    }

    // ===================== CAMERA =====================

    private void CenterCamera()
    {
        if (mainCamera == null)
            return;

        float worldWidth = width * cellSize;
        float worldHeight = height * cellSize;

        mainCamera.transform.position = new Vector3(
            worldWidth / 2f,
            worldHeight / 2f,
            -10f
        );
    }
}
