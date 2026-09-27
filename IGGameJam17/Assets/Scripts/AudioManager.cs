using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Mix")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;

    [Header("SFX Variation")]
    [SerializeField] private bool varyPitch = true;
    [SerializeField] private float pitchJitter = 0.05f;

    [Header("Individual Clips")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip foodCollect;
    [SerializeField] private AudioClip trashCollect;
    [SerializeField] private AudioClip wrongSort;
    [SerializeField] private AudioClip[] pickup;

    public void PlayButtonClick()
    {
        PlaySound(buttonClick);
    }

    public void PlayFoodCollect()
    {
        PlaySound(foodCollect);
    }

    public void PlayTrashCollect()
    {
        PlaySound(trashCollect, 0.5f);
    }

    public void PlayWrongSort()
    {
        PlaySound(wrongSort);
    }

    public void PlayPickup()
    {
        PlayRandom(pickup);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
    }

    // ============================================================
    //  RESTART
    // ============================================================
    /// <summary>
    /// Stops all currently-playing SFX and optionally music.
    /// Call from your restart flow before reloading the scene.
    /// </summary>
    public static void Restart(bool stopMusic = true)
    {
        if (Instance == null) return;

        if (Instance.sfxSource != null)
            Instance.sfxSource.Stop();

        if (stopMusic && Instance.musicSource != null)
            Instance.musicSource.Stop();
    }

    // ============================================================
    //  STATIC CONVENIENCE WRAPPERS
    // ============================================================
    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (Instance == null) return;
        Instance.PlaySound(clip, volume, pitch);
    }

    public static void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (Instance == null) return;
        Instance.PlaySoundAt(clip, position, volume, pitch);
    }

    public static void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.PlayMusicInternal(clip, volume);
    }

    public static void StopMusic()
    {
        if (Instance == null) return;
        Instance.musicSource.Stop();
    }

    // ============================================================
    //  RANDOM CLIP SELECTION (uniform)
    // ============================================================
    /// <summary>
    /// Plays one clip chosen uniformly at random from the array.
    /// Each clip has equal probability; immediate repeats are possible.
    /// </summary>
    public static void PlayRandom(AudioClip[] clips, float volume = 1f, float pitch = 1f)
    {
        if (Instance == null) return;
        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length); // [0, length) — uniform
        var clip = clips[index];
        if (clip != null) Instance.PlaySound(clip, volume, pitch);
    }

    /// <summary>
    /// Plays one clip chosen uniformly at random from the list.
    /// </summary>
    public static void PlayRandom(IList<AudioClip> clips, float volume = 1f, float pitch = 1f)
    {
        if (Instance == null) return;
        if (clips == null || clips.Count == 0) return;

        int index = Random.Range(0, clips.Count);
        var clip = clips[index];
        if (clip != null) Instance.PlaySound(clip, volume, pitch);
    }

    // ============================================================
    //  INSTANCE PLAYBACK
    // ============================================================
    public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxSource == null) return;

        float finalPitch = pitch;
        if (varyPitch && Mathf.Approximately(pitch, 1f))
            finalPitch = 1f + Random.Range(-pitchJitter, pitchJitter);

        sfxSource.pitch = finalPitch;
        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volume);
    }

    public void PlaySoundAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, masterVolume * sfxVolume * volume);
    }

    private void PlayMusicInternal(AudioClip clip, float volume)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = masterVolume * musicVolume * volume;
        musicSource.Play();
    }

    // ============================================================
    //  VOLUME SETTERS
    // ============================================================
    public static void SetMasterVolume(float v) { if (Instance != null) Instance.masterVolume = Mathf.Clamp01(v); }
    public static void SetSfxVolume(float v) { if (Instance != null) Instance.sfxVolume = Mathf.Clamp01(v); }
    public static void SetMusicVolume(float v) { if (Instance != null) Instance.musicVolume = Mathf.Clamp01(v); }
}