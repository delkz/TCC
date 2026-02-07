using UnityEngine;
using UnityEngine.SceneManagement;
using System;


public class GameSession : MonoBehaviour
{
    public static GameSession Instance;

    public LevelData SelectedLevel { get; private set; }

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // chamado pelo menu
    public void SelectLevel(LevelData level)
    {
        SelectedLevel = level;

        Debug.Log($"Level selecionado: {level.levelName}");

        // ⚠️ TEMPORÁRIO PARA TESTE
        SceneManager.LoadScene("GamePlay");
    }

    // útil no futuro (retry, menu, profile)
    public void ClearSelectedLevel()
    {
        SelectedLevel = null;
    }
}
