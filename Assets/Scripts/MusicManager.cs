using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameMusicOrAmbience; // your SampleScene ambience/music

    [Header("Fade")]
    [Range(0f, 1f)] public float musicVolume = 0.35f;
    public float fadeTime = 1.25f;

    private AudioSource a;   // current
    private AudioSource b;   // next (for crossfade)
    private bool usingA = true;

    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Two AudioSources for crossfading
        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();

        SetupSource(a);
        SetupSource(b);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void SetupSource(AudioSource src)
    {
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D music
        src.volume = 0f;
    }

    void Start()
    {
        // Start on whatever scene is currently loaded
        PlayForScene(SceneManager.GetActiveScene().name, instant: true);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name, instant: false);
    }

    void PlayForScene(string sceneName, bool instant)
    {
        AudioClip target =
            (sceneName == "MainMenu") ? mainMenuMusic :
            (sceneName == "SampleScene") ? gameMusicOrAmbience :
            null;

        if (target == null) return;

        AudioSource current = usingA ? a : b;
        // If already playing this clip, do nothing
        if (current.clip == target && current.isPlaying) return;

        if (instant)
        {
            // Hard switch (only on first load)
            a.Stop(); b.Stop();
            usingA = true;
            a.clip = target;
            a.volume = musicVolume;
            a.Play();
            b.volume = 0f;
            return;
        }

        // Crossfade to target
        StartCoroutine(CrossfadeTo(target));
    }

    IEnumerator CrossfadeTo(AudioClip nextClip)
    {
        AudioSource from = usingA ? a : b;
        AudioSource to   = usingA ? b : a;

        to.clip = nextClip;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float startFrom = from.volume;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = t / fadeTime;

            from.volume = Mathf.Lerp(startFrom, 0f, k);
            to.volume   = Mathf.Lerp(0f, musicVolume, k);

            yield return null;
        }

        from.Stop();
        from.volume = 0f;
        to.volume = musicVolume;

        usingA = !usingA;
    }
}