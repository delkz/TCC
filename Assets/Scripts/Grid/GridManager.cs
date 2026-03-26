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
    private GridMapAsset mapAsset;

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    // ===================== STATE =====================

    private int width;
    private int height;

    private GridCell[,] grid;
    private GameObject[,] groundLayer;
    private GameObject[,] pathLayer;
    private GameObject[,] blockedLayer;

    private Vector2Int spawnCell;
    private Vector2Int goalCell;

    // ===================== UNITY =====================

    private void Awake()
    {
        if (GameSession.Instance == null || GameSession.Instance.SelectedLevel == null)
        {
            Debug.LogWarning("Gameplay iniciada sem LevelData (modo debug).");
            return;
        }


        LevelData level = GameSession.Instance.SelectedLevel;

        if (level == null || level.map == null)
        {
            Debug.LogError("GridManager: LevelData ou mapa não definido.");
            return;
        }

        mapAsset = level.map;
        LoadMap(level.map);
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
        groundLayer = new GameObject[width, height];
        pathLayer = new GameObject[width, height];
        blockedLayer = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GridCell cell = grid[x, y];

                Vector3 pos = GridToWorldCenter(new Vector2Int(x, y));

                // =====================
                // GROUND LAYER
                // =====================

                GameObject ground = CreateTile(
                    pos,
                    mapAsset.theme.emptySprite,
                    "Ground",
                    0
                );

                groundLayer[x, y] = ground;

                // =====================
                // PATH LAYER
                // =====================

                if (cell.type == CellType.Path)
                {
                    GameObject path = CreateTile(
                        pos,
                        GetPathSprite(x, y),
                        "Path",
                        1
                    );

                    pathLayer[x, y] = path;
                }

                // =====================
                // BLOCKED LAYER
                // =====================

                if (cell.type == CellType.Blocked)
                {
                    GameObject blocked = CreateTile(
                        pos,
                        mapAsset.theme.blockedSprite,
                        "Blocked",
                        2
                    );

                    blockedLayer[x, y] = blocked;
                }

                // =====================
                // SPAWN / GOAL
                // =====================

                if (cell.type == CellType.Spawn)
                {
                    CreateTile(
                        pos,
                        mapAsset.theme.spawnSprite,
                        "Spawn",
                        3
                    );
                }

                if (cell.type == CellType.Goal)
                {
                    CreateTile(
                        pos,
                        mapAsset.theme.goalSprite,
                        "Goal",
                        3
                    );
                }
            }
        }
    }


    private void ApplyTheme(SpriteRenderer sr, CellType type, int x, int y)
    {
        GridTheme theme = mapAsset.theme;

        if (theme == null)
        {
            sr.color = Color.magenta;
            return;
        }

        Sprite sprite = type switch
        {
            CellType.Path => GetPathSprite(x, y),
            CellType.Spawn => theme.spawnSprite,
            CellType.Goal => theme.goalSprite,
            CellType.Blocked => theme.blockedSprite,
            _ => theme.emptySprite
        };

        sr.sprite = sprite;
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

        GridObject gridObject = spawnGO.GetComponent<GridObject>();
        if (gridObject != null)
        {
            gridObject.AlignToGrid(cellSize);
        }

        SetCellOccupant(spawnCell, spawnGO);
    }

    private void SpawnNexus()
    {
        GameObject nexus = Instantiate(
            nexusPrefab,
            GridToWorldCenter(goalCell),
            Quaternion.identity
        );

        GridObject gridObject = nexus.GetComponent<GridObject>();
        if (gridObject != null)
        {
            gridObject.AlignToGrid(cellSize);
        }

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
    private bool IsPath(int x, int y)
    {
        if (!IsValidCell(x, y))
            return false;

        return grid[x, y].IsPath || grid[x, y].IsGoal || grid[x, y].IsSpawn;
    }
    private GameObject CreateTile(Vector3 pos, Sprite sprite, string name, int sortingOrder)
    {
        GameObject tile = Instantiate(
            gridTilePrefab,
            pos,
            Quaternion.identity,
            transform
        );

        tile.name = $"{name}_{pos.x}_{pos.y}";

        GridObject gridObject = tile.GetComponent<GridObject>();
        if (gridObject != null)
            gridObject.AlignToGrid(cellSize);

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        return tile;
    }
    // ===================== PATH (LINEAR, MAP-DEFINED) =====================
    private Sprite GetPathSprite(int x, int y)
    {
        bool up = IsPath(x, y + 1);
        bool down = IsPath(x, y - 1);
        bool left = IsPath(x - 1, y);
        bool right = IsPath(x + 1, y);

        int connections =
            (up ? 1 : 0) +
            (down ? 1 : 0) +
            (left ? 1 : 0) +
            (right ? 1 : 0);

        GridTheme theme = mapAsset.theme;

        return connections switch
        {
            4 => theme.pathCross,
            3 => theme.pathT,
            2 when (up && down) || (left && right) => theme.pathStraight,
            2 => theme.pathCorner,
            _ => theme.pathEnd
        };
    }
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

        // Centralizar
        mainCamera.transform.position = new Vector3(
            worldWidth / 2f,
            worldHeight / 2f,
            -10f
        );

        // Ajustar zoom para caber toda grid
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = worldWidth / worldHeight;

        if (screenRatio >= targetRatio)
        {
            mainCamera.orthographicSize = worldHeight / 2f;
        }
        else
        {
            float difference = targetRatio / screenRatio;
            mainCamera.orthographicSize = worldHeight / 2f * difference;
        }
    }
    private void Update()
    {
        if (ScreenSizeChanged())
        {
            CenterCamera();
        }
    }

    private Vector2 lastScreenSize;

    private bool ScreenSizeChanged()
    {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            return true;
        }

        return false;
    }
}
