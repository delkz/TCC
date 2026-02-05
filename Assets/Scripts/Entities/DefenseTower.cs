using UnityEngine;
using System.Collections.Generic;

public class DefenseTower : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int damage = 1;

    [Header("Effects")]
    [SerializeField] private float knockback = 0f;
    [SerializeField] private float slowAmount = 0f;
    [SerializeField] private float slowDuration = 0f;

    [Header("Targeting")]
    [SerializeField] private TowerTargetMode targetMode = TowerTargetMode.LowestHealth;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    private float cooldownTimer;
    private List<Enemy> enemiesInRange = new();

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            Enemy target = SelectTarget();
            if (target != null)
            {
                Shoot(target);
                cooldownTimer = attackCooldown;
            }
        }
    }

    private void Shoot(Enemy target)
    {
        Projectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        projectile.Initialize(
            target,
            damage,
            knockback,
            slowAmount,
            slowDuration
        );
    }

    // ================= TARGET SELECTION =================

    private Enemy SelectTarget()
    {
        if (enemiesInRange.Count == 0)
            return null;

        return targetMode switch
        {
            TowerTargetMode.Closest => GetClosestEnemy(),
            TowerTargetMode.LowestHealth => GetLowestHealthEnemy(),
            _ => null
        };
    }

    private Enemy GetLowestHealthEnemy()
    {
        Enemy target = null;
        int lowestHealth = int.MaxValue;

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemiesInRange[i];

            if (enemy == null)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }

            if (enemy.CurrentHealth < lowestHealth)
            {
                lowestHealth = enemy.CurrentHealth;
                target = enemy;
            }
        }

        return target;
    }

    private Enemy GetClosestEnemy()
    {
        Enemy target = null;
        float closestDistance = float.MaxValue;

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemiesInRange[i];

            if (enemy == null)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = enemy;
            }
        }

        return target;
    }

    // ================= RANGE =================

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemiesInRange.Contains(enemy))
            enemiesInRange.Add(enemy);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
            enemiesInRange.Remove(enemy);
    }
}
