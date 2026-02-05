using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{
    public event Action OnGridChanged;
    [Header("Grid Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;

    [Header("World References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject worldSprite;

    [Header("Map Entities")]
    [SerializeField] private GameObject nexusPrefab;
    [SerializeField] private GameObject enemySpawnPrefab;

    [Header("Enemies")]
    [SerializeField] private Enemy enemyPrefab;
    // ===================== DATA =====================

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

    private class PathNode
    {
        public int x;
        public int y;
        public int gCost;
        public int hCost;
        public int fCost;
        public PathNode cameFromNode;
        public bool walkable;

        public PathNode(int x, int y, bool walkable)
        {
            this.x = x;
            this.y = y;
            this.walkable = walkable;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }


    // Preparação para Save / Load
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

    // ===================== UNITY =====================

    private void Awake()
    {
        CreateGrid();
        SpawnNexus();
        SpawnEnemySpawnPoint();
    }

    // ===================== GRID =====================

    private void CreateGrid()
    {
        grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y);
            }
        }

        Debug.Log($"Grid criado: {width} x {height}");

        CenterCamera();
        ResizeWorld();
    }

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool IsCellBuildable(int x, int y)
    {
        if (!IsValidCell(x, y))
            return false;

        return !grid[x, y].isOccupied && grid[x, y].isBuildable;
    }

    public void SetCellOccupied(int x, int y, bool occupied)
    {
        if (!IsValidCell(x, y))
            return;
        grid[x, y].isOccupied = occupied;
        OnGridChanged?.Invoke();
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
            gridPos.x + cellSize / 2f,
            gridPos.y + cellSize / 2f,
            0f
        );
    }

    public bool CanBlockCell(Vector2Int cellToBlock)
    {
        // célula inválida ou já ocupada
        if (!IsValidCell(cellToBlock.x, cellToBlock.y))
            return false;

        if (grid[cellToBlock.x, cellToBlock.y].isOccupied)
            return false;

        // salva estado atual
        bool previousState = grid[cellToBlock.x, cellToBlock.y].isOccupied;

        // simula bloqueio
        grid[cellToBlock.x, cellToBlock.y].isOccupied = true;

        Vector2Int start = GetEnemySpawnCell();
        Vector2Int end = GetGridCenterCell();

        var path = FindPath(start, end);

        // desfaz simulação
        grid[cellToBlock.x, cellToBlock.y].isOccupied = previousState;

        // caminho válido existe?
        return path != null && path.Count > 0;
    }


    // ===================== MAP ENTITIES =====================

    private void SpawnNexus()
    {
        if (nexusPrefab == null)
        {
            Debug.LogWarning("Nexus prefab não atribuído.");
            return;
        }

        Vector2Int cell = GetGridCenterCell();

        Instantiate(
            nexusPrefab,
            GridToWorldCenter(cell),
            Quaternion.identity
        );

        SetCellOccupied(cell.x, cell.y, true);
    }

    private void SpawnEnemySpawnPoint()
    {
        if (enemySpawnPrefab == null || enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawn ou Enemy prefab não atribuído.");
            return;
        }

        Vector2Int cell = GetEnemySpawnCell();

        GameObject spawnGO = Instantiate(
            enemySpawnPrefab,
            GridToWorldCenter(cell),
            Quaternion.identity
        );

        EnemySpawnPoint spawnPoint = spawnGO.GetComponent<EnemySpawnPoint>();
        Nexus nexus = FindObjectOfType<Nexus>();

        if (spawnPoint == null || nexus == null)
        {
            Debug.LogError("Erro ao inicializar EnemySpawnPoint.");
            return;
        }

        spawnPoint.Initialize(
            enemyPrefab,
            nexus,
            this // 🔹 GridManager da cena
        );

        SetCellOccupied(cell.x, cell.y, true);
    }



    // ===================== POSITIONS =====================

    public Vector2Int GetGridCenterCell()
    {
        return new Vector2Int(
            Mathf.FloorToInt(width / 2f),
            Mathf.FloorToInt(height / 2f)
        );
    }

    public Vector2Int GetEnemySpawnCell()
    {
        return new Vector2Int(
            0,
            Mathf.FloorToInt(height / 2f)
        );
    }

    private List<PathNode> FindPathInternal(Vector2Int start, Vector2Int end)
    {
        PathNode[,] nodes = new PathNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nodes[x, y] = new PathNode(
                    x,
                    y,
                    !grid[x, y].isOccupied
                );
            }
        }

        PathNode startNode = nodes[start.x, start.y];
        PathNode endNode = nodes[end.x, end.y];

        startNode.walkable = true;
        endNode.walkable = true;

        List<PathNode> openList = new() { startNode };
        HashSet<PathNode> closedList = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PathNode node = nodes[x, y];
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
                return CalculatePath(endNode);

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbor in GetNeighbors(currentNode, nodes))
            {
                if (closedList.Contains(neighbor))
                    continue;

                if (!neighbor.walkable)
                {
                    closedList.Add(neighbor);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistance(currentNode, neighbor);

                if (tentativeGCost < neighbor.gCost)
                {
                    neighbor.cameFromNode = currentNode;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = CalculateDistance(neighbor, endNode);
                    neighbor.CalculateFCost();

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<PathNode> GetNeighbors(PathNode node, PathNode[,] nodes)
    {
        List<PathNode> neighbors = new();

        if (node.x - 1 >= 0) neighbors.Add(nodes[node.x - 1, node.y]);
        if (node.x + 1 < width) neighbors.Add(nodes[node.x + 1, node.y]);
        if (node.y - 1 >= 0) neighbors.Add(nodes[node.x, node.y - 1]);
        if (node.y + 1 < height) neighbors.Add(nodes[node.x, node.y + 1]);

        return neighbors;
    }

    private int CalculateDistance(PathNode a, PathNode b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private PathNode GetLowestFCostNode(List<PathNode> nodes)
    {
        PathNode lowest = nodes[0];

        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fCost < lowest.fCost)
                lowest = nodes[i];
        }

        return lowest;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new();
        PathNode current = endNode;

        while (current != null)
        {
            path.Add(current);
            current = current.cameFromNode;
        }

        path.Reverse();
        return path;
    }


    public List<Vector3> FindPath(Vector2Int start, Vector2Int end)
    {
        List<PathNode> path = FindPathInternal(start, end);
        if (path == null)
            return null;

        List<Vector3> worldPath = new();

        foreach (PathNode node in path)
        {
            worldPath.Add(GridToWorldCenter(new Vector2Int(node.x, node.y)));
        }

        return worldPath;
    }


    // ===================== VISUAL =====================

    private void CenterCamera()
    {
        if (mainCamera == null)
            return;

        mainCamera.transform.position = new Vector3(
            width / 2f,
            height / 2f,
            -10f
        );
    }

    private void ResizeWorld()
    {
        if (worldSprite == null)
            return;

        Transform t = worldSprite.transform;
        t.localScale = new Vector3(width, height, 1f);
        t.position = new Vector3(width / 2f, height / 2f, 0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = new Vector3(
                    x + cellSize / 2f,
                    y + cellSize / 2f,
                    0f
                );

                Gizmos.DrawWireCube(center, Vector3.one * cellSize);
            }
        }
    }
}
