using System;
using UnityEngine;

public enum GameAudioEventType
{
    EnemyHit,
    LevelCompleted,
    GameOver,
    BuildPlaced,
    BuildRemoved
}

public readonly struct GameAudioEvent
{
    public GameAudioEventType Type { get; }
    public Vector3 Position { get; }
    public bool HasPosition { get; }

    public GameAudioEvent(GameAudioEventType type, Vector3 position, bool hasPosition)
    {
        Type = type;
        Position = position;
        HasPosition = hasPosition;
    }
}

public static class GameAudioEvents
{
    public static event Action<GameAudioEvent> OnEvent;

    public static void RaiseEnemyHit(Vector3 worldPosition)
    {
        Raise(new GameAudioEvent(GameAudioEventType.EnemyHit, worldPosition, true));
    }

    public static void RaiseLevelCompleted()
    {
        Raise(new GameAudioEvent(GameAudioEventType.LevelCompleted, default, false));
    }

    public static void RaiseGameOver()
    {
        Raise(new GameAudioEvent(GameAudioEventType.GameOver, default, false));
    }

    public static void RaiseBuildPlaced(Vector3 worldPosition)
    {
        Raise(new GameAudioEvent(GameAudioEventType.BuildPlaced, worldPosition, true));
    }

    public static void RaiseBuildRemoved(Vector3 worldPosition)
    {
        Raise(new GameAudioEvent(GameAudioEventType.BuildRemoved, worldPosition, true));
    }

    public static void Raise(GameAudioEvent audioEvent)
    {
        OnEvent?.Invoke(audioEvent);
    }
}