using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private int health = 3;
    public int CurrentHealth => health;

    [SerializeField] private int damageToNexus = 1;
    private List<Vector3> path;
    private int pathIndex;

    private Transform target;

    private GridManager gridManager;
    private Vector2Int targetCell;

    public void SetTarget(Transform nexus)
    {
        target = nexus;
    }

    private void Update()
    {
        if (path == null || pathIndex >= path.Count)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            path[pathIndex],
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, path[pathIndex]) < 0.05f)
        {
            pathIndex++;
        }
    }


    public void TakeDamage(int damage)
    {

        health -= damage;
        Debug.Log("I took damage, my life is now " + health);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void Initialize(GridManager gridManager, Vector2Int targetCell)
    {
        this.gridManager = gridManager;
        this.targetCell = targetCell;

        gridManager.OnGridChanged += RecalculatePath;
        RecalculatePath();
    }

    private void OnDestroy()
    {
        if (gridManager != null)
            gridManager.OnGridChanged -= RecalculatePath;
    }
    private void RecalculatePath()
    {
        Vector2Int currentCell = gridManager.WorldToGridPosition(transform.position);
        var newPath = gridManager.FindPath(currentCell, targetCell);

        if (newPath == null || newPath.Count == 0)
            return;

        path = newPath;
        pathIndex = 0;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Nexus nexus = other.GetComponent<Nexus>();
        if (nexus == null)
            return;

        nexus.TakeDamage(damageToNexus);
        Destroy(gameObject);
    }
}
