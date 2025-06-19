using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicFaderManager : MonoBehaviour
{
    public static MusicFaderManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private List<AudioClip> playlist;
    [SerializeField] private float delayBetweenTracks = 1f;

    private Coroutine playlistCoroutine;
    private Coroutine fadeCoroutine;

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

    public void StartPlaylist()
    {
        if (playlistCoroutine != null)
            StopCoroutine(playlistCoroutine);

        playlistCoroutine = StartCoroutine(PlaylistRoutine());
    }

    public void StopPlaylist()
    {
        if (playlistCoroutine != null)
            StopCoroutine(playlistCoroutine);

        FadeOut();
    }

    private IEnumerator PlaylistRoutine()
    {
        while (true)
        {
            if (playlist.Count == 0)
                yield break;

            AudioClip randomClip = playlist[Random.Range(0, playlist.Count)];
            yield return StartCoroutine(FadeInCoroutine(randomClip, 1f));

            yield return new WaitForSeconds(randomClip.length - fadeDuration);
            yield return StartCoroutine(FadeOutCoroutine());

            yield return new WaitForSeconds(delayBetweenTracks);
        }
    }

    private IEnumerator FadeInCoroutine(AudioClip clip, float volume)
    {
        if (audioSource.isPlaying)
            yield return StartCoroutine(FadeOutCoroutine());

        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = volume;
    }

    private IEnumerator FadeOutCoroutine()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    public void FadeOut() => StartCoroutine(FadeOutCoroutine());
}
