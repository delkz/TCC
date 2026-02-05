using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Nexus : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    public int currentHealth { get; private set; }

    public event Action<int> OnHealthChanged;
    public event Action OnNexusDestroyed;


    private void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"Nexus recebeu dano! Vida: {currentHealth}");

        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("Nexus destruído! Reiniciando cena...");
        OnNexusDestroyed?.Invoke();

        // Reinicia a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}