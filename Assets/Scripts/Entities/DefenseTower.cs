using UnityEngine;
using System.Collections.Generic;

public class DefenseTower : MonoBehaviour
{
    private Buildable buildable;
    private BuildingData data;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Rotation")]
    [SerializeField] private Transform rotatingPart;
    private float cooldownTimer;
    private readonly List<Enemy> enemiesInRange = new();

    private void Awake()
    {
        buildable = GetComponent<Buildable>();
        data = buildable.Data;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        Enemy target = SelectTarget();

        if (target != null)
        {
            RotateTowards(target);

            if (cooldownTimer <= 0f)
            {
                Shoot(target);
                cooldownTimer = data.attackCooldown;
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
            data.damage,
            data.knockback,
            data.slowAmount,
            data.slowDuration
        );
    }

    // ================= TARGET SELECTION =================

    private Enemy SelectTarget()
    {
        if (enemiesInRange.Count == 0)
            return null;

        Enemy closest = null;
        float minDistance = float.MaxValue;

        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy == null)
                continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy;
            }
        }

        return closest;
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

    // ================= ANIMATION =================

    private void RotateTowards(Enemy target)
    {
        if (rotatingPart == null || target == null)
            return;

        Vector3 direction = target.transform.position - rotatingPart.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rotatingPart.rotation = Quaternion.Lerp(
            rotatingPart.rotation,
            Quaternion.Euler(0f, 0f, angle - 90f),
            Time.deltaTime * 10f
        );

    }

}
