using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("ID único e imutável. Nunca mude depois de publicado.")]
    public string levelId;

    public string levelName;

    [TextArea]
    public string description;

    [Header("Map")]
    public GridMapAsset map;

    [Header("Economy")]
    public int startingGold = 60;

    [Header("Difficulty")]
    public float enemyHealthMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;

    [Header("Win / Lose")]
    public int nexusHealth = 10;

    [Header("Unlock Rules")]
    [Tooltip("IDs de níveis que precisam ser concluídos antes")]
    public string[] requiredCompletedLevels;
}
