using UnityEngine;
using System.Collections.Generic;

public class DefenseTower : MonoBehaviour, IRangeDisplayable
{
    private Buildable buildable;
    private CircleCollider2D rangeCollider;

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
        rangeCollider = FindRangeCollider();
        ApplyRangeFromBuildable();
    }

    private void OnEnable()
    {
        if (buildable != null)
        {
            buildable.OnLevelChanged += HandleLevelChanged;
        }
    }

    private void OnDisable()
    {
        if (buildable != null)
        {
            buildable.OnLevelChanged -= HandleLevelChanged;
        }
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
                cooldownTimer = buildable != null ? buildable.AttackCooldown : 1f;
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

        if (buildable == null)
        {
            return;
        }

        projectile.Initialize(
            target,
            buildable.Damage,
            buildable.Knockback,
            buildable.SlowAmount,
            buildable.SlowDuration
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

    private void HandleLevelChanged(int _)
    {
        ApplyRangeFromBuildable();
    }

    private CircleCollider2D FindRangeCollider()
    {
        CircleCollider2D[] colliders = GetComponentsInChildren<CircleCollider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].isTrigger)
            {
                return colliders[i];
            }
        }

        return null;
    }

    private void ApplyRangeFromBuildable()
    {
        if (buildable == null || rangeCollider == null)
        {
            return;
        }

        rangeCollider.radius = Mathf.Max(0f, buildable.Range);
    }

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

    // ================= INTERFACE: IRangeDisplayable =================

    public float GetDisplayRange()
    {
        if (buildable == null)
        {
            return 0f;
        }

        return buildable.Range;
    }

    public Vector3 GetDisplayPosition()
    {
        return transform.position;
    }

}
