using Ami.BroAudio;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound IDs")]
    [SerializeField] private SoundID backgroundMusic;
    [SerializeField] private SoundID enemyHitSfx;
    [SerializeField] private SoundID levelCompletedSfx;
    [SerializeField] private SoundID gameOverSfx;
    [SerializeField] private SoundID buildPlacedSfx;
    [SerializeField] private SoundID buildRemovedSfx;

    [Header("Volume")]
    [SerializeField, Range(0f, 10f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 10f)] private float sfxVolume = 1f;
    [SerializeField] private float volumeFadeTime = 0.15f;

    [Header("Startup")]
    [SerializeField] private bool playBgmOnStart = true;

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

    private void OnEnable()
    {
        GameAudioEvents.OnEvent += HandleAudioEvent;
    }

    private void OnDisable()
    {
        GameAudioEvents.OnEvent -= HandleAudioEvent;
    }

    private void Start()
    {
        ApplyVolumes();

        if (playBgmOnStart)
        {
            PlayMusic();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp(value, 0f, 10f);
        BroAudio.SetVolume(BroAudioType.Music, musicVolume, volumeFadeTime);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp(value, 0f, 10f);
        BroAudio.SetVolume(BroAudioType.SFX, sfxVolume, volumeFadeTime);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }

    private void ApplyVolumes()
    {
        BroAudio.SetVolume(BroAudioType.Music, musicVolume, volumeFadeTime);
        BroAudio.SetVolume(BroAudioType.SFX, sfxVolume, volumeFadeTime);
    }

    private void HandleAudioEvent(GameAudioEvent audioEvent)
    {
        switch (audioEvent.Type)
        {
            case GameAudioEventType.EnemyHit:
                PlayEnemyHit(audioEvent);
                break;
            case GameAudioEventType.LevelCompleted:
                PlayIfValid(levelCompletedSfx);
                break;
            case GameAudioEventType.GameOver:
                PlayIfValid(gameOverSfx);
                break;
            case GameAudioEventType.BuildPlaced:
                PlaySfxAtEventPosition(audioEvent, buildPlacedSfx);
                break;
            case GameAudioEventType.BuildRemoved:
                PlaySfxAtEventPosition(audioEvent, buildRemovedSfx);
                break;
        }
    }

    private void PlayEnemyHit(GameAudioEvent audioEvent)
    {
        PlaySfxAtEventPosition(audioEvent, enemyHitSfx);
    }

    private static void PlaySfxAtEventPosition(GameAudioEvent audioEvent, SoundID sfxId)
    {
        if (sfxId.Equals(SoundID.Invalid))
        {
            return;
        }

        if (audioEvent.HasPosition)
        {
            BroAudio.Play(sfxId, audioEvent.Position);
            return;
        }

        BroAudio.Play(sfxId);
    }

    private void PlayMusic()
    {
        if (backgroundMusic.Equals(SoundID.Invalid))
        {
            return;
        }

        BroAudio.Play(backgroundMusic).AsBGM();
    }

    private static void PlayIfValid(SoundID id)
    {
        if (id.Equals(SoundID.Invalid))
        {
            return;
        }

        BroAudio.Play(id);
    }
}