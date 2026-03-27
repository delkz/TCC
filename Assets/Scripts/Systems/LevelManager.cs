using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Level Manager")]
public class LevelManager : ScriptableObject
{
    [SerializeField] private List<LevelData> levels = new();

    public List<LevelData> GetAllLevels() => new(levels);

    public int GetLevelCount() => levels.Count;

    public LevelData GetLevel(int index)
    {
        if (index >= 0 && index < levels.Count)
            return levels[index];

        Debug.LogError($"LevelManager: Indice de nivel invalido: {index}");
        return null;
    }

    public LevelData GetLevelById(string levelId)
    {
        foreach (var level in levels)
        {
            if (level != null && level.levelId == levelId)
                return level;
        }

        Debug.LogWarning($"LevelManager: Level com ID '{levelId}' nao encontrado");
        return null;
    }

    #if UNITY_EDITOR
    public void AddLevel(LevelData level)
    {
        if (level != null && !levels.Contains(level))
            levels.Add(level);
    }

    public void RemoveLevel(LevelData level)
    {
        if (level != null)
            levels.Remove(level);
    }
    #endif
}
