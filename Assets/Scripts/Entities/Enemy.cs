using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public enum EnemyState
{
    Idle,
    Walking,
    Attacking,
    Defending,
    Dead
}
public class Enemy : MonoBehaviour
{
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private int prize = 5;
    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    public EnemyState CurrentState => currentState;
    public int GetDamage() => damage;
    public int GetPrize() => prize;

    public event Action<Enemy, EnemyDeathReason> OnEnemyDied;

    private float currentHealth;
    private float slowMultiplier = 1f;

    private List<Vector2Int> path;
    private int pathIndex;
    private GridManager gridManager;

    private bool isDead;

    // ===================== INIT =====================

    public void Initialize(List<Vector2Int> path, GridManager gridManager)
    {
        this.path = path;
        this.gridManager = gridManager;
        animator = GetComponent<Animator>();

        pathIndex = 0;
        currentHealth = maxHealth;
        slowMultiplier = 1f;

        transform.position = gridManager.GridToWorldCenter(path[0]);

        SetState(EnemyState.Walking);
    }
    private void Update()
    {
        if (isDead) return;

        MoveAlongPath();
    }

    public void ApplyDifficulty(LevelData level)
    {
        maxHealth *= level.enemyHealthMultiplier;
        speed *= level.enemySpeedMultiplier;
        currentHealth = maxHealth;
    }

    // ===================== ANIMATION =====================
    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        ApplyStateAnimation();
    }
    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        ApplyStateAnimation();
    }
    private void ApplyStateAnimation()
    {
        if (!HasAnimation("Walk")) return;
        
        if (animator == null)
            animator = GetComponent<Animator>();

        switch (currentState)
        {
            case EnemyState.Idle:
                animator.Play("Idle");
                break;

            case EnemyState.Walking:
                animator.Play("Walk");
                break;

            case EnemyState.Attacking:
                animator.Play("Attack");
                break;

            case EnemyState.Defending:
                animator.Play("Defend");
                break;

            case EnemyState.Dead:
                animator.Play("Death");
                break;
        }
    }
    private bool HasAnimation(string animationName)
    {
        if (animator == null) return false;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
                return true;
        }

        return false;
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

            if (pathIndex >= path.Count)
            {
                SetState(EnemyState.Attacking);

                if (HasAnimation("Attack"))
                    StartCoroutine(DieAfterAttack());
                else
                    Die(EnemyDeathReason.ReachedGoal);
            }
        }
    }

    // ===================== COMBAT =====================

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            isDead = true;

            SetState(EnemyState.Dead);

            if (HasAnimation("Death"))
                StartCoroutine(DieAfterDeath());
            else
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

    private IEnumerator SlowRoutine(float slowAmount, float duration)
    {
        slowMultiplier = Mathf.Clamp01(1f - slowAmount);
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1f;
    }

    // ===================== DEATH =====================

    private IEnumerator DieAfterAttack()
    {
        yield return new WaitForSeconds(0.6f);
        Die(EnemyDeathReason.ReachedGoal);
    }

    private IEnumerator DieAfterDeath()
    {
        yield return new WaitForSeconds(0.5f);
        Die(EnemyDeathReason.KilledByPlayer);
    }

    private void Die(EnemyDeathReason reason)
    {
        OnEnemyDied?.Invoke(this, reason);
        Destroy(gameObject);
    }
}