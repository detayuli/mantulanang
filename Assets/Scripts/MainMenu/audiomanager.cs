using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class audiomanager : MonoBehaviour
{
    public static audiomanager Instance { get; private set; }

    [Header("---------- Audio Sources ----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---------- Audio Clips ----------")]
    public AudioClip HamsterTewas;
    public AudioClip HamsterRespawn;
    public AudioClip CollideBorder;
    public AudioClip CollideHamster;
    public AudioClip BGMMusic;
    public AudioClip VictoryMusic;

    [Header("---------- Scene Music Mapping ----------")]
    [SerializeField] private List<SceneMusicMapping> sceneMusicMappings;

    private Dictionary<string, AudioClip> sceneMusicDict = new Dictionary<string, AudioClip>();
    private bool isWalking = false;
    private bool sfxEnabled = true;
    private bool musicEnabled = true;

    [System.Serializable]
    public class SceneMusicMapping
    {
        public List<string> sceneNames;
        public AudioClip musicClip;
    }

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Simpan scene-music mapping ke dictionary
        foreach (var mapping in sceneMusicMappings)
        {
            foreach (var sceneName in mapping.sceneNames)
            {
                sceneMusicDict[sceneName] = mapping.musicClip;
            }
        }

        // Subscribe ke event scene change
        SceneManager.activeSceneChanged += OnSceneChanged;

        // Mainkan musik untuk scene awal
        PlayMusicInCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // ===============================
    // ======== MUSIC CONTROL ========
    // ===============================

    private void OnSceneChanged(Scene previousScene, Scene newScene)
    {
        PlayMusicInCurrentScene();
    }

    public void PlayMusicInCurrentScene()
    {
        if (!musicEnabled || musicSource == null) return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (sceneMusicDict.TryGetValue(currentScene, out AudioClip targetClip))
        {
            if (musicSource.clip == targetClip && musicSource.isPlaying)
                return; // Musik sama, biarkan

            musicSource.clip = targetClip;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            musicSource.Stop(); // Tidak ada musik untuk scene ini
        }
    }

    public void PlayCustomMusic(AudioClip clip)
    {
        if (!musicEnabled || clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;

        if (musicSource != null)
            musicSource.mute = !enabled;

        // Jika dimatikan, stop langsung
        if (!enabled)
            musicSource.Stop();
        else
            PlayMusicInCurrentScene();
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    // ===============================
    // ========= SFX CONTROL ==========
    // ===============================

    public void SetSFXEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        if (SFXSource != null)
            SFXSource.mute = !enabled;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXSource != null && clip != null && sfxEnabled)
            SFXSource.PlayOneShot(clip);
    }

    // ===============================
    // ======== EXTRA HANDLER =========
    // ===============================

    public void HandleWalking(bool walking)
    {
        if (SFXSource == null) return;

        if (walking && !isWalking)
        {
            SFXSource.clip = BGMMusic;
            SFXSource.loop = true;
            SFXSource.Play();
            isWalking = true;
        }
        else if (!walking && isWalking)
        {
            SFXSource.Stop();
            isWalking = false;
        }
    }

    public void HandleButtonPress()
    {
        PlaySFX(HamsterTewas);
    }
}
