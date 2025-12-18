using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbienceAudio : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(Fade(0f, duration));
    }

    public void FadeIn(float targetVolume, float duration)
    {
        StartCoroutine(Fade(targetVolume, duration));
    }

    private System.Collections.IEnumerator Fade(float target, float time)
    {
        float start = audioSource.volume;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, target, t / time);
            yield return null;
        }

        audioSource.volume = target;
    }
}
