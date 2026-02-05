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
    [Header("Timing Balance")]
    [SerializeField] private float initialWaveDelay = 10f;
    [SerializeField] private float timePerEnemy = 10f;
    private int currentWave = 0;
    private bool waveInProgress;
    private float waveTimer;
    private float maxWaveDuration;
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
        yield return new WaitForSeconds(initialWaveDelay);
        StartNextWave();
    }
    private void StartNextWave()
    {
        currentWave++;

        enemiesRemaining = startingEnemies + (currentWave - 1) * 2;

        maxWaveDuration = enemiesRemaining * timePerEnemy;
        waveTimer = maxWaveDuration;

        waveInProgress = true;

        Debug.Log(
            $"Wave {currentWave} iniciada | Inimigos: {enemiesRemaining} | Tempo máximo: {maxWaveDuration}s"
        );

        StartCoroutine(SpawnWave(enemiesRemaining));
    }
    private void Update()
    {
        if (!waveInProgress)
            return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f)
        {
            Debug.Log($"Wave {currentWave} forçada pelo tempo!");
            ForceEndWave();
        }
    }
    private void ForceEndWave()
    {
        waveInProgress = false;

        // opcional: limpar lista para não bloquear próxima wave
        aliveEnemies.Clear();

        StartCoroutine(WaitForNextWave());
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

    private void HandleEnemyDeath(Enemy enemy, EnemyDeathReason reason)
    {
        aliveEnemies.Remove(enemy);

        if (reason == EnemyDeathReason.KilledByTower && goldManager != null)
        {
            goldManager.Add(goldPerEnemy);
        }

        if (aliveEnemies.Count == 0 && waveInProgress)
        {
            waveInProgress = false;
            StartCoroutine(WaitForNextWave());
        }
    }


    private IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }
}
