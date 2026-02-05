using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnPoint : MonoBehaviour
{
    [System.Serializable]
    public class SpawnOption
    {
        public Enemy enemyPrefab;
        [Min(0f)] public float weight = 1f;
    }

    [Header("Spawn Options")]
    [SerializeField] private List<SpawnOption> spawnOptions = new();

    private GridManager gridManager;
    private Nexus nexus;

    public void Initialize(Nexus nexus, GridManager gridManager)
    {
        this.nexus = nexus;
        this.gridManager = gridManager;
    }


    public Enemy SpawnEnemy()
    {
        if (spawnOptions.Count == 0)
        {
            Debug.LogWarning("EnemySpawnPoint sem opções de spawn.");
            return null;
        }

        Enemy enemyPrefab = PickEnemyByWeight();
        if (enemyPrefab == null)
            return null;

        Enemy enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        enemy.Initialize(
            gridManager,
            gridManager.GetGridCenterCell()
        );

        return enemy;
    }

    // ================= WEIGHTED RANDOM =================

    private Enemy PickEnemyByWeight()
    {
        float totalWeight = 0f;

        foreach (var option in spawnOptions)
        {
            if (option.enemyPrefab != null && option.weight > 0f)
                totalWeight += option.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float random = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var option in spawnOptions)
        {
            if (option.enemyPrefab == null || option.weight <= 0f)
                continue;

            current += option.weight;

            if (random <= current)
                return option.enemyPrefab;
        }

        // fallback (não deveria acontecer)
        return spawnOptions[0].enemyPrefab;
    }
}
