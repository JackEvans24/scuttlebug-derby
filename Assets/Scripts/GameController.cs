using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    [SerializeField] private int positionWaitFrames = 1;
    public static int PositionWaitFrames { get => _instance.positionWaitFrames; }

    private PlayerSelection[] players;
    public static PlayerSelection[] Players { get => _instance.players; }

    private MusicLoop music;
    private SoundController sound;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

        this.music = GetComponentInChildren<MusicLoop>();
        this.sound = GetComponentInChildren<SoundController>();
    }

    public static void SetPlayersAndPlay(IEnumerable<PlayerSelection> players, Scenes level)
    {
        _instance.players = players.ToArray();
        SceneManager.LoadScene((int)level);
    }

    public static void UpdateMusic(MusicTrack track, bool play = true) => _instance.music.UpdateTrack(track, play);

    public static void PlayMusic() => _instance.music.Play();

    public static Guid PlaySound(AudioClip clip) => _instance.sound.PlayClip(clip);
    public static void StopSound(Guid id) => _instance.sound.StopSound(id);
}
