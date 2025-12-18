using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManagerSingle : MonoBehaviour
{
    public static MusicManagerSingle Instance;

    [Header("Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameMusicOrAmbience;

    [Header("Fade")]
    [Range(0f, 1f)] public float musicVolume = 0.05f;
    public float fadeTime = 1.0f;

    [Header("Delay")]
    public float gameStartDelay = 1.0f; // silence before ambience

    private AudioSource audioSource;
    private Coroutine routine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        PlayForScene(SceneManager.GetActiveScene(), instant: true);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (routine != null) StopCoroutine(routine);

        if (scene.name == "SampleScene")
        {
            // Fade out menu immediately, then delay ambience
            routine = StartCoroutine(FadeOutThenDelayPlay(scene));
        }
        else
        {
            PlayForScene(scene, instant: false);
        }
    }

    IEnumerator FadeOutThenDelayPlay(Scene scene)
    {
        // Fade out whatever is currently playing (menu music)
        float startVol = audioSource.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        audioSource.Stop();

        // Silence
        yield return new WaitForSeconds(gameStartDelay);

        // Start ambience
        PlayForScene(scene, instant: true);
    }

    void PlayForScene(Scene scene, bool instant)
    {
        AudioClip target = (scene.buildIndex == 0)
            ? mainMenuMusic
            : gameMusicOrAmbience;

        if (target == null) return;

        if (audioSource.clip == target && audioSource.isPlaying) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SwitchWithFade(target, instant));
    }

    IEnumerator SwitchWithFade(AudioClip next, bool instant)
    {
        if (instant)
        {
            audioSource.clip = next;
            audioSource.volume = musicVolume;
            audioSource.Play();
            yield break;
        }

        // Fade out
        float startVol = audioSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = next;
        audioSource.Play();

        // Fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeTime);
            yield return null;
        }

        audioSource.volume = musicVolume;
    }
}