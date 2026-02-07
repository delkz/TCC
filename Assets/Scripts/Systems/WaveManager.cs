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
    [SerializeField] private Nexus nexus;

    private void Start()
    {
        StartCoroutine(WaitForSpawnerAndStart());
    }

    private IEnumerator WaitForSpawnerAndStart()
    {
        while (spawnPoint == null && nexus == null)
        {
            spawnPoint = FindObjectOfType<EnemySpawnPoint>();
            nexus = FindObjectOfType<Nexus>();
            yield return null; // espera próximo frame
        }

        UILogger.Log("EnemySpawnPoint e Nexus encontrados. Iniciando waves.");
        UILogger.Log(
            $"Wave iniciando em {initialWaveDelay} segundos... Prepare-se!"
        );
        yield return new WaitForSeconds(initialWaveDelay);
        StartNextWave();
    }
    private void StartNextWave()
    {
        currentWave++;

        enemiesRemaining = startingEnemies + (currentWave - 1) * 2;

        // maxWaveDuration = enemiesRemaining * timePerEnemy;
        maxWaveDuration = Mathf.Floor(30f + (enemiesRemaining * timePerEnemy / 3f)); // ajuste para balancear
        waveTimer = maxWaveDuration;

        waveInProgress = true;

        UILogger.Log(
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
            UILogger.Log($"Wave {currentWave} for�ada pelo tempo!");
            ForceEndWave();
        }
    }
    private void ForceEndWave()
    {
        waveInProgress = false;

        // opcional: limpar lista para n�o bloquear pr�xima wave
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
        enemiesRemaining--;

        UILogger.Log($"Inimigo morto | Razão: {reason} | Inimigos restantes: {enemiesRemaining} | Dano recebido: {enemy.GetDamage()}");

        if (reason == EnemyDeathReason.KilledByPlayer)
        {
            goldManager.Add(goldPerEnemy);
        }
        else if (reason == EnemyDeathReason.ReachedGoal)
        {
            nexus.TakeDamage(enemy.GetDamage());
        }

        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        // Se ainda há inimigos vivos, a wave continua
        if (aliveEnemies.Count > 0)
            return;

        // Se ainda estamos spawnando inimigos, também não finaliza
        if (waveInProgress && enemiesRemaining > aliveEnemies.Count)
            return;

        UILogger.Log($"Wave {currentWave} finalizada com sucesso!");

        waveInProgress = false;
        StartCoroutine(WaitForNextWave());
    }


    private IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }
}
