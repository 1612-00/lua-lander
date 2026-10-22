using System;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance { get; private set; }

    private static int musicVolume = 6;
    private const int MUSIC_VOLUME_MAX = 10;

    public event EventHandler OnMusicVolumeChanged;

    private AudioSource musicAudioSource;
    private static float musicTime;

    private void Awake() {
        Instance = this;
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.time = musicTime;
    }

    private void Update() {
        musicTime = musicAudioSource.time;
    }

    public void ChangeMusicVolume() {
        musicVolume = (musicVolume + 1) % MUSIC_VOLUME_MAX;
        musicAudioSource.volume = GetMusicVolumeNormalized();
        OnMusicVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetMusicVolume() {
        return musicVolume;
    }

    public float GetMusicVolumeNormalized() {
        return (float)musicVolume / MUSIC_VOLUME_MAX;
    }
}
