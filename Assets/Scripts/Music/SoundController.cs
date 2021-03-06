using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviour
{
    [Header("General SFX")]
    [SerializeField, Range(0, 1)] private float volume = 0.2f;

    [Header("Footstep sounds")]
    [SerializeField, Range(0, 1)] private float stepVolume = 0.2f;
    [SerializeField] private GameObject stepSourceObject;
    [SerializeField] private AudioClip[] stepSounds;
    private AudioSource[] stepSources;

    private AudioSource[] sources;
    private Dictionary<Guid, AudioSource> playingSources;

    private void Awake()
    {
        this.sources = GetComponents<AudioSource>();
        this.playingSources = new Dictionary<Guid, AudioSource>();

        foreach (var source in this.sources)
            source.volume = this.volume;

        this.stepSources = this.stepSourceObject.GetComponents<AudioSource>();
        foreach (var source in this.stepSources)
            source.volume = this.stepVolume;
    }

    public Guid PlayClip(AudioClip clip)
    {
        if (clip == null)
            return Guid.Empty;

        var availableSource = this.sources.FirstOrDefault(s => !s.isPlaying);
        if (availableSource == null)
            return Guid.Empty;

        var id = Guid.NewGuid();
        this.playingSources.Add(id, availableSource);

        availableSource.clip = clip;
        availableSource.Play();

        StartCoroutine(this.StopAfter(id, clip.length));

        return id;
    }

    public IEnumerator PlayClipAsCoroutine(AudioClip clip)
    {
        if (clip == null)
            yield break;

        var availableSource = this.sources.FirstOrDefault(s => !s.isPlaying);
        if (availableSource == null)
            yield break;

        var id = Guid.NewGuid();
        this.playingSources.Add(id, availableSource);

        availableSource.clip = clip;
        availableSource.Play();

        yield return StopAfter(id, clip.length);
    }

    public void PlayStep()
    {
        var availableSource = this.stepSources.FirstOrDefault(s => !s.isPlaying);
        if (availableSource == null)
            return;

        var clip = this.stepSounds[UnityEngine.Random.Range(0, this.stepSounds.Length)];
        availableSource.clip = clip;
        availableSource.Play();
    }

    private IEnumerator StopAfter(Guid id, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        this.StopSound(id);
    }

    public void StopSound(Guid sourceId)
    {
        if (!this.playingSources.TryGetValue(sourceId, out var source))
            return;

        source.Stop();
        this.playingSources.Remove(sourceId);
    }

    public void StopAllSounds()
    {
        var keys = this.playingSources.Select(source => source.Key).ToList();
        foreach (var key in keys)
            this.StopSound(key);
    }

    public void PauseAllSounds()
    {
        foreach (var kvp in this.playingSources)
            kvp.Value.Pause();
    }

    public void ResumeAllSounds()
    {
        foreach (var kvp in this.playingSources)
            kvp.Value.UnPause();
    }
}
