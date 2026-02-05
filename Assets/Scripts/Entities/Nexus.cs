using UnityEngine;

public class Nexus : MonoBehaviour
{
    [SerializeField] private int health = 10;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Nexus recebeu dano! Vida: {health}");

        if (health <= 0)
        {
            Debug.Log("Nexus destruído! Game Over");
        }
    }
}
