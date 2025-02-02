using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioSource soundEffectSource;
    private static AudioSource musicSource;
    public float musicVolume = 1f;
    public float effectsVolume = 1f;

    private void Awake()
    {
        // Ensure only one instance of AudioController exists
        if (FindObjectsByType<AudioController>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Persist across scenes

        // Create and configure the sound effect AudioSource
        soundEffectSource = gameObject.AddComponent<AudioSource>();
        soundEffectSource.playOnAwake = false;
        soundEffectSource.loop = false;

        // Create and configure the music AudioSource
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }

    private void Start()
    {
        SetMusicVolume(musicVolume);
        SetSoundVolume(effectsVolume);
    }

    /// <summary>
    /// Play a sound effect once.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="volume">The volume of the sound (default is 1).</param>
    public static void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioController: No sound clip provided.");
            return;
        }

        soundEffectSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Play background music. If music is already playing, it will be replaced.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="volume">The volume of the music (default is 1).</param>
    public static void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioController: No music clip provided.");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            Debug.Log("AudioController: Music is already playing.");
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
    }

    /// <summary>
    /// Stop the currently playing background music.
    /// </summary>
    public static void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Pause the currently playing background music.
    /// </summary>
    public static void PauseMusic()
    {
        musicSource.Pause();
    }

    /// <summary>
    /// Resume the paused background music.
    /// </summary>
    public static void ResumeMusic()
    {
        musicSource.UnPause();
    }

    /// <summary>
    /// Set the volume of the background music.
    /// </summary>
    /// <param name="volume">The volume level (0 to 1).</param>
    public static void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }

    /// <summary>
    /// Set the volume of the sound effects.
    /// </summary>
    /// <param name="volume">The volume level (0 to 1).</param>
    public static void SetSoundVolume(float volume)
    {
        soundEffectSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }
}