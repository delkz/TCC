using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Serializable]
    private class CellPrefabBinding
    {
        public CellType type;
        public GameObject prefab;
        public bool occupyCell = true;
        public bool parentToGrid = false;
    }

    public event Action OnGridChanged;

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private GameObject nexusPrefab;
    [SerializeField] private GameObject enemySpawnPrefab;

    [Header("Cell Type Prefabs")]
    [SerializeField] private List<CellPrefabBinding> cellPrefabBindings = new();

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private GridMapAsset mapAsset;

    private int width;
    private int height;

    private GridCell[,] grid;
    private GameObject[,] groundLayer;
    private GameObject[,] pathLayer;
    private GameObject[,] contentLayer;

    private Vector2Int spawnCell;
    private Vector2Int goalCell;
    private Vector2 lastScreenSize;

    public float CellSize => cellSize;

    private void Awake()
    {
        if (!TryGetSelectedMap(out GridMapAsset selectedMap))
        {
            return;
        }

        LoadMap(selectedMap);
    }

    private void Update()
    {
        if (ScreenSizeChanged())
        {
            CenterCamera();
        }
    }

    public void LoadMap(GridMapAsset map)
    {
        if (!ValidateMap(map))
        {
            return;
        }

        InitializeMapState(map);
        BuildMapRuntime();
    }

    private bool ValidateMap(GridMapAsset map)
    {
        if (map != null)
        {
            return true;
        }

        Debug.LogError("GridManager: MapAsset não atribuído.");
        return false;
    }

    private void InitializeMapState(GridMapAsset map)
    {
        mapAsset = map;
        width = map.width;
        height = map.height;
    }

    private void BuildMapRuntime()
    {
        CreateGridFromMap(mapAsset);
        GenerateGridVisual();
        SpawnMapEntities();
        CenterCamera();
        DebugPath();
    }

    private bool TryGetSelectedMap(out GridMapAsset selectedMap)
    {
        selectedMap = null;

        if (GameSession.Instance == null || GameSession.Instance.SelectedLevel == null)
        {
            Debug.LogWarning("Gameplay iniciada sem LevelData (modo debug).");
            return false;
        }

        LevelData level = GameSession.Instance.SelectedLevel;
        if (level == null || level.map == null)
        {
            Debug.LogError("GridManager: LevelData ou mapa não definido.");
            return false;
        }

        selectedMap = level.map;
        return true;
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
                RegisterSpecialCell(type, x, y);
            }
        }
    }

    private void RegisterSpecialCell(CellType type, int x, int y)
    {
        if (type == CellType.Spawn)
        {
            spawnCell = new Vector2Int(x, y);
        }

        if (type == CellType.Goal)
        {
            goalCell = new Vector2Int(x, y);
        }
    }

    private void GenerateGridVisual()
    {
        groundLayer = new GameObject[width, height];
        pathLayer = new GameObject[width, height];
        contentLayer = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCellVisual(x, y);
            }
        }
    }

    private void CreateCellVisual(int x, int y)
    {
        GridCell cell = grid[x, y];
        Vector3 worldPosition = GridToWorldCenter(new Vector2Int(x, y));

        CreateGroundVisual(x, y, worldPosition);
        CreatePathVisualIfNeeded(cell, x, y, worldPosition);
        CreateCellContentVisualIfNeeded(cell, x, y, worldPosition);
    }

    private void CreateGroundVisual(int x, int y, Vector3 worldPosition)
    {
        groundLayer[x, y] = CreateTile(worldPosition, mapAsset.theme.emptySprite, "Ground", 0);
    }

    private void CreatePathVisualIfNeeded(GridCell cell, int x, int y, Vector3 worldPosition)
    {
        if (cell.type != CellType.Path)
        {
            return;
        }

        pathLayer[x, y] = CreateTile(worldPosition, GetPathSprite(x, y), "Path", 1);
    }

    private void CreateCellContentVisualIfNeeded(GridCell cell, int x, int y, Vector3 worldPosition)
    {
        if (cell.type == CellType.Empty || cell.type == CellType.Path)
        {
            return;
        }

        if (mapAsset.theme == null)
        {
            return;
        }

        if (!mapAsset.theme.TryGetSprite(cell.type, out Sprite sprite, out int sortingOrder))
        {
            return;
        }

        string tileName = cell.type.ToString();
        contentLayer[x, y] = CreateTile(worldPosition, sprite, tileName, sortingOrder);
    }

    private void ApplyTheme(SpriteRenderer sr, CellType type, int x, int y)
    {
        GridTheme theme = mapAsset.theme;

        if (theme == null)
        {
            sr.color = Color.magenta;
            return;
        }

        Sprite sprite = type == CellType.Path
            ? GetPathSprite(x, y)
            : null;

        if (sprite == null)
        {
            theme.TryGetSprite(type, out sprite, out _);
        }

        if (sprite == null)
        {
            sprite = theme.emptySprite;
        }

        sr.sprite = sprite;
    }

    private void SpawnMapEntities()
    {
        SpawnEnemySpawnPoint();
        SpawnNexus();
        SpawnBoundCellPrefabs();
    }

    private void SpawnEnemySpawnPoint()
    {
        SpawnEntityAtCell(enemySpawnPrefab, spawnCell, true);
    }

    private void SpawnNexus()
    {
        SpawnEntityAtCell(nexusPrefab, goalCell, true);
    }

    private void SpawnEntityAtCell(GameObject prefab, Vector2Int cellPosition, bool setAsOccupant)
    {
        if (prefab == null)
        {
            Debug.LogWarning("GridManager: prefab de entidade não atribuído.");
            return;
        }

        GameObject entity = Instantiate(prefab, GridToWorldCenter(cellPosition), Quaternion.identity);

        if (entity.TryGetComponent(out GridObject gridObject))
        {
            gridObject.AlignToGrid(cellSize);
        }

        if (setAsOccupant)
        {
            SetCellOccupant(cellPosition, entity);
        }
    }

    private void SpawnBoundCellPrefabs()
    {
        if (cellPrefabBindings == null || cellPrefabBindings.Count == 0)
        {
            return;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                CellType type = grid[x, y].type;

                if (!TryGetCellPrefabBinding(type, out CellPrefabBinding binding))
                {
                    continue;
                }

                SpawnBoundPrefab(binding, cellPosition);
            }
        }
    }

    private bool TryGetCellPrefabBinding(CellType type, out CellPrefabBinding binding)
    {
        for (int i = 0; i < cellPrefabBindings.Count; i++)
        {
            CellPrefabBinding candidate = cellPrefabBindings[i];
            if (candidate != null && candidate.type == type && candidate.prefab != null)
            {
                binding = candidate;
                return true;
            }
        }

        binding = null;
        return false;
    }

    private void SpawnBoundPrefab(CellPrefabBinding binding, Vector2Int cellPosition)
    {
        GameObject entity;
        Vector3 worldPosition = GridToWorldCenter(cellPosition);

        if (binding.parentToGrid)
        {
            entity = Instantiate(binding.prefab, worldPosition, Quaternion.identity, transform);
        }
        else
        {
            entity = Instantiate(binding.prefab, worldPosition, Quaternion.identity);
        }

        entity.name = $"{binding.type}_{cellPosition.x}_{cellPosition.y}";

        if (entity.TryGetComponent(out GridObject gridObject))
        {
            gridObject.AlignToGrid(cellSize);
        }

        if (binding.occupyCell)
        {
            SetCellOccupant(cellPosition, entity);
        }
    }

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool IsCellBuildable(int x, int y)
    {
        if (!IsValidCell(x, y))
        {
            return false;
        }

        GridCell cell = grid[x, y];
        return !cell.IsOccupied && cell.isBuildable;
    }

    public void SetCellOccupant(Vector2Int pos, GameObject occupant)
    {
        if (!IsValidCell(pos.x, pos.y))
        {
            return;
        }

        grid[pos.x, pos.y].occupant = occupant;
        OnGridChanged?.Invoke();
    }

    public GameObject GetCellOccupant(Vector2Int pos)
    {
        if (!IsValidCell(pos.x, pos.y))
        {
            return null;
        }

        return grid[pos.x, pos.y].occupant;
    }

    private bool IsPath(int x, int y)
    {
        if (!IsValidCell(x, y))
        {
            return false;
        }

        return grid[x, y].IsPath || grid[x, y].IsGoal || grid[x, y].IsSpawn;
    }

    private GameObject CreateTile(Vector3 pos, Sprite sprite, string name, int sortingOrder)
    {
        GameObject tile = Instantiate(gridTilePrefab, pos, Quaternion.identity, transform);
        tile.name = $"{name}_{pos.x}_{pos.y}";

        if (tile.TryGetComponent(out GridObject gridObject))
        {
            gridObject.AlignToGrid(cellSize);
        }

        if (tile.TryGetComponent(out SpriteRenderer sr))
        {
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }

        return tile;
    }

    private Sprite GetPathSprite(int x, int y)
    {
        PathNeighbors neighbors = GetPathNeighbors(x, y);
        int connections = CountConnections(neighbors);
        return ResolvePathSprite(connections, neighbors);
    }

    private PathNeighbors GetPathNeighbors(int x, int y)
    {
        return new PathNeighbors(
            IsPath(x, y + 1),
            IsPath(x, y - 1),
            IsPath(x - 1, y),
            IsPath(x + 1, y)
        );
    }

    private int CountConnections(PathNeighbors neighbors)
    {
        return
            (neighbors.Up ? 1 : 0) +
            (neighbors.Down ? 1 : 0) +
            (neighbors.Left ? 1 : 0) +
            (neighbors.Right ? 1 : 0);
    }

    private Sprite ResolvePathSprite(int connections, PathNeighbors neighbors)
    {
        GridTheme theme = mapAsset.theme;

        return connections switch
        {
            4 => theme.pathCross,
            3 => theme.pathT,
            2 when (neighbors.Up && neighbors.Down) || (neighbors.Left && neighbors.Right) => theme.pathStraight,
            2 => theme.pathCorner,
            _ => theme.pathEnd
        };
    }

    public List<Vector2Int> BuildPath()
    {
        List<Vector2Int> path = new();
        HashSet<Vector2Int> visited = new();

        Vector2Int current = spawnCell;
        RegisterPathStep(path, visited, current);

        while (current != goalCell)
        {
            Vector2Int next = GetNextPathCell(current, visited);

            if (PathTraversalStuck(current, next))
            {
                Debug.LogError("GridManager: Caminho inválido no mapa.");
                break;
            }

            current = next;
            RegisterPathStep(path, visited, current);
        }

        return path;
    }

    private void RegisterPathStep(List<Vector2Int> path, HashSet<Vector2Int> visited, Vector2Int cell)
    {
        path.Add(cell);
        visited.Add(cell);
    }

    private bool PathTraversalStuck(Vector2Int current, Vector2Int next)
    {
        return next == current;
    }

    private Vector2Int GetNextPathCell(Vector2Int current, HashSet<Vector2Int> visited)
    {
        foreach (Vector2Int direction in CardinalDirections)
        {
            Vector2Int next = current + direction;

            if (!IsValidCell(next.x, next.y))
            {
                continue;
            }

            if (visited.Contains(next))
            {
                continue;
            }

            if (grid[next.x, next.y].IsPath || grid[next.x, next.y].IsGoal)
            {
                return next;
            }
        }

        return current;
    }

    private void DebugPath()
    {
        foreach (Vector2Int cell in BuildPath())
        {
            Debug.Log($"Path cell: {cell}");
        }
    }

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

    private void CenterCamera()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector2 worldSize = GetWorldSize();
        CenterCameraPosition(worldSize);
        AdjustCameraZoom(worldSize);
    }

    private Vector2 GetWorldSize()
    {
        return new Vector2(width * cellSize, height * cellSize);
    }

    private void CenterCameraPosition(Vector2 worldSize)
    {
        mainCamera.transform.position = new Vector3(worldSize.x / 2f, worldSize.y / 2f, -10f);
    }

    private void AdjustCameraZoom(Vector2 worldSize)
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = worldSize.x / worldSize.y;

        if (screenRatio >= targetRatio)
        {
            mainCamera.orthographicSize = worldSize.y / 2f;
            return;
        }

        float difference = targetRatio / screenRatio;
        mainCamera.orthographicSize = worldSize.y / 2f * difference;
    }

    private bool ScreenSizeChanged()
    {
        Vector2 currentScreenSize = GetCurrentScreenSize();
        if (!HasScreenSizeChanged(currentScreenSize))
        {
            return false;
        }

        lastScreenSize = currentScreenSize;
        return true;
    }

    private Vector2 GetCurrentScreenSize()
    {
        return new Vector2(Screen.width, Screen.height);
    }

    private bool HasScreenSizeChanged(Vector2 currentScreenSize)
    {
        return lastScreenSize.x != currentScreenSize.x || lastScreenSize.y != currentScreenSize.y;
    }

    private readonly struct PathNeighbors
    {
        public readonly bool Up;
        public readonly bool Down;
        public readonly bool Left;
        public readonly bool Right;

        public PathNeighbors(bool up, bool down, bool left, bool right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
    }
}
