using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    private Enemy enemyPrefab;
    private Nexus nexus;
    private GridManager gridManager;

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

    public Enemy SpawnEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        enemy.Initialize(
            gridManager,
            gridManager.GetGridCenterCell()
        );

        return enemy;
    }
}
