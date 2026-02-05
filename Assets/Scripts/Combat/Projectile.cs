using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;

    private Enemy target;
    private int damage;
    private float knockback;
    private float slowAmount;
    private float slowDuration;

    public void Initialize(
        Enemy target,
        int damage,
        float knockback,
        float slowAmount,
        float slowDuration
    )
    {
        this.target = target;
        this.damage = damage;
        this.knockback = knockback;
        this.slowAmount = slowAmount;
        this.slowDuration = slowDuration;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            speed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || enemy != target)
            return;

        enemy.TakeDamage(damage);

        if (knockback > 0f)
            enemy.ApplyKnockback(transform.position, knockback);

        if (slowAmount > 0f)
            enemy.ApplySlow(slowAmount, slowDuration);

        Destroy(gameObject);
    }
}
