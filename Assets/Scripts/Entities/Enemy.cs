using UnityEngine;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private int prize = 5;
    public int GetDamage() => damage;
    public int GetPrize() => prize;
    public event Action<Enemy, EnemyDeathReason> OnEnemyDied;

    private float currentHealth;
    private float slowMultiplier = 1f;

    private List<Vector2Int> path;
    private int pathIndex;
    private GridManager gridManager;

    // ===================== INIT =====================

    public void Initialize(List<Vector2Int> path, GridManager gridManager)
    {
        this.path = path;
        this.gridManager = gridManager;

        pathIndex = 0;
        currentHealth = maxHealth;
        slowMultiplier = 1f;

        transform.position = gridManager.GridToWorldCenter(path[0]);
    }

    private void Update()
    {
        MoveAlongPath();
    }

    // ===================== MOVEMENT =====================

    private void MoveAlongPath()
    {
        if (path == null || pathIndex >= path.Count)
            return;

        Vector3 target = gridManager.GridToWorldCenter(path[pathIndex]);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * slowMultiplier * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            pathIndex++;

            // chegou ao final do caminho
            if (pathIndex >= path.Count)
            {
                Die(EnemyDeathReason.ReachedGoal);
            }
        }
    }

    // ===================== COMBAT =====================

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die(EnemyDeathReason.KilledByPlayer);
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        transform.position += direction.normalized * force;
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(slowAmount, duration));
    }

    private System.Collections.IEnumerator SlowRoutine(float slowAmount, float duration)
    {
        slowMultiplier = Mathf.Clamp01(1f - slowAmount);
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1f;
    }

    // ===================== DEATH =====================

    private void Die(EnemyDeathReason reason)
    {
        OnEnemyDied?.Invoke(this, reason);
        Destroy(gameObject);
    }
}
