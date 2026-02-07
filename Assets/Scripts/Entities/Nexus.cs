using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Nexus : MonoBehaviour
{
    private int maxHealth = 10;

    public int currentHealth { get; private set; }

    public event Action<int> OnHealthChanged;
    public event Action OnNexusDestroyed;

    public void Initialize(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Awake()
    {
        if (GameSession.Instance == null || GameSession.Instance.SelectedLevel == null)
        {
            Debug.LogWarning("Gameplay iniciada sem LevelData (modo debug).");
            return;
        }


        LevelData level = GameSession.Instance.SelectedLevel;

        if (level == null)
            return;

        Initialize(level.nexusHealth);
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
        OnNexusDestroyed?.Invoke();

        // Reinicia a cena atual
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}