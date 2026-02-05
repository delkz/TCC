using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int startingEnemies = 5;
    [SerializeField] private float spawnInterval = 0.8f;
    [SerializeField] private float timeBetweenWaves = 3f;
    [Header("Gold Settings")]
    [SerializeField] private GoldManager goldManager;
    [SerializeField] private int goldPerEnemy = 5;
    private EnemySpawnPoint spawnPoint;

    private int currentWave = 0;
    private bool waveInProgress;

    private readonly List<Enemy> aliveEnemies = new();
    private int enemiesRemaining;


    private void Start()
    {
        StartCoroutine(WaitForSpawnerAndStart());
    }
    private IEnumerator WaitForSpawnerAndStart()
    {
        while (spawnPoint == null)
        {
            spawnPoint = FindObjectOfType<EnemySpawnPoint>();
            yield return null; // espera próximo frame
        }

        Debug.Log("EnemySpawnPoint encontrado. Iniciando waves.");
        StartNextWave();
    }
    private void StartNextWave()
    {
        currentWave++;
        int enemiesToSpawn = startingEnemies + (currentWave - 1) * 2;
        waveInProgress = true;

        Debug.Log($"Wave {currentWave} iniciada. Inimigos: {enemiesToSpawn}");

        StartCoroutine(SpawnWave(enemiesToSpawn));
    }

    private IEnumerator SpawnWave(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Enemy enemy = spawnPoint.SpawnEnemy();
            aliveEnemies.Add(enemy);

            enemy.OnEnemyDied += HandleEnemyDeath;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        aliveEnemies.Remove(enemy);

        if (goldManager != null)
        {
            goldManager.Add(goldPerEnemy);
        }

        if (aliveEnemies.Count == 0 && waveInProgress)
        {
            waveInProgress = false;
            Debug.Log($"Wave {currentWave} finalizada!");
            StartCoroutine(WaitForNextWave());
        }
    }


    private IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }
}
