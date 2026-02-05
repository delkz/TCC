using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 2f;

    private Enemy enemyPrefab;
    private Nexus nexus;
    private GridManager gridManager;

    private float timer;

    public void Initialize(
        Enemy enemyPrefab,
        Nexus nexus,
        GridManager gridManager
    )
    {
        this.enemyPrefab = enemyPrefab;
        this.nexus = nexus;
        this.gridManager = gridManager;
    }

    private void Update()
    {
        if (enemyPrefab == null || nexus == null || gridManager == null)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        Vector2Int start = gridManager.GetEnemySpawnCell();
        Vector2Int end = gridManager.GetGridCenterCell();

        List<Vector3> path = gridManager.FindPath(start, end);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("Nenhum caminho encontrado para o inimigo.");
            return;
        }

        enemy.Initialize(
            gridManager,
            gridManager.GetGridCenterCell()
        );
    }
}
