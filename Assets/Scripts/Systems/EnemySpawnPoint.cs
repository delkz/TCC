using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnPoint : MonoBehaviour
{
    [System.Serializable]
    public class WeightedEnemy
    {
        public Enemy enemyPrefab;
        public int weight = 1;
    }

    [Header("Enemy Pool")]
    [SerializeField] private WeightedEnemy[] enemies;

    private GridManager gridManager;
    private List<Vector2Int> path;

    private void Awake()
    {
        // resolve GridManager automaticamente
        gridManager = FindFirstObjectByType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("EnemySpawnPoint: GridManager não encontrado.");
            return;
        }

        path = gridManager.BuildPath();
    }

    public Enemy SpawnEnemy()
    {
        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogError("EnemySpawnPoint: Nenhum inimigo configurado.");
            return null;
        }

        Enemy prefab = PickWeightedEnemy();
        Enemy enemy = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity
        );

        enemy.Initialize(path, gridManager);
        enemy.ApplyDifficulty(GameSession.Instance.SelectedLevel);
        return enemy;
    }

    // ===================== WEIGHTED PICK =====================

    private Enemy PickWeightedEnemy()
    {
        int totalWeight = 0;

        foreach (var entry in enemies)
            totalWeight += entry.weight;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var entry in enemies)
        {
            current += entry.weight;
            if (random < current)
                return entry.enemyPrefab;
        }

        // fallback (não deve acontecer)
        return enemies[0].enemyPrefab;
    }
}
