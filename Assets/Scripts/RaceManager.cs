using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInputManager))]
public class RaceManager : MonoBehaviour
{
    private static RaceManager _instance;

    private PlayerInputManager inputManager;

    private List<CarMovement> racers;
    private Dictionary<CarMovement, int> racePositions = new Dictionary<CarMovement, int>();
    private List<CarMovement> finishedRacers = new List<CarMovement>();
    private int totalPlayerCount;
    private int finishedPlayers;

    private List<RaceTextController> raceTextControllers;

    [Header("References")]
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject PlayerContainer;
    [SerializeField] private GameObject AI;
    [SerializeField] private Transform startLine;
    [SerializeField] private MusicTrack music;

    [Header("Race Variables")]
    [SerializeField] private Color backgroundColor;
    [ColorUsage(showAlpha: true, hdr: true)]
    [SerializeField] private Color lightingColor;

    [Header("Race Start")]
    [SerializeField] private CameraDolly[] introCameras;
    [SerializeField] private AudioClip introTune;
    [SerializeField] private AudioClip startShot;
    [SerializeField] private Vector3 startPositioning;
    [SerializeField] private ParticleSystem[] startParticles;

    [Header("Race position")]
    [SerializeField] private int positionUpdateFrequency = 5;

    [Header("Race finish")]
    [SerializeField] private ParticleSystem[] finishParticles;
    [SerializeField] private AudioClip finishSound;

    private int totalRacerCount, currentFrameCount;
    private bool setupComplete, raceComplete;

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

        this.inputManager = GetComponent<PlayerInputManager>();
    }

    private void Start()
    {
        GameController.UpdateMusic(music, false);
    }

    private void Update()
    {
        if (!this.setupComplete)
            return;

        this.currentFrameCount++;
        if (this.currentFrameCount >= this.positionUpdateFrequency)
        {
            this.currentFrameCount = 0;
            this.UpdatePositions();
        }
    }

    public static IEnumerator StartRaceSetup() => _instance.RaceSetup(GameController.Players, GameController.TotalRacers);

    public static void StartRace() => _instance.StartCoroutine(_instance.StartRaceLocal());

    private IEnumerator RaceSetup(PlayerSelection[] players, int totalRacerCount)
    {
        this.totalRacerCount = totalRacerCount;
        this.totalPlayerCount = players.Length;

        RenderSettings.ambientLight = this.lightingColor;

        this.GenerateAiRacers(this.totalRacerCount - players.Length);
        this.GeneratePlayers(players);

        yield return this.PositionRacers();

        if (this.introCameras.Length > 0)
            this.introCameras[0].Prewarm();

        this.setupComplete = true;
    }

    private void GenerateAiRacers(int racerCount)
    {
        this.racers = new List<CarMovement>();

        for (int i = 0; i < racerCount; i++)
        {
            var position = this.GetPosition(i);

            var racer = Instantiate(this.AI, position, this.startLine.rotation);
            var carMovement = racer.GetComponent<CarMovement>();

            this.racers.Add(carMovement);
        }
    }

    private void GeneratePlayers(PlayerSelection[] players)
    {
        if (racers == null)
            this.racers = new List<CarMovement>();
        this.raceTextControllers = new List<RaceTextController>();

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            var input = this.inputManager.JoinPlayer(playerIndex: i, pairWithDevice: player.Device);
            var racer = input.transform;

            // Set up camera
            var racerContainer = racer.parent;
            var camera = racerContainer.GetComponentInChildren<Camera>();
            camera.backgroundColor = this.backgroundColor;

            // Set up player skin
            var mesh = racer.GetComponentInChildren<SkinnedMeshRenderer>();
            mesh.sharedMesh = player.BodyModel;
            mesh.materials[0].color = player.MainColor;
            mesh.materials[1].color = player.AccentColor;

            // Set up race controller variables
            var carMovement = racer.GetComponentInChildren<CarMovement>();
            var textController = racerContainer.GetComponentInChildren<RaceTextController>();

            this.racers.Add(carMovement);
            this.raceTextControllers.Add(textController);
        }
    }

    private IEnumerator PositionRacers()
    {
        for (int i = 0; i < 5; i++)
            yield return new WaitForEndOfFrame();

        for (int i = 0; i < this.racers.Count; i++)
        {
            var position = this.GetPosition(i);

            var racer = this.racers[i];

            racer.MoveTo(position);
            racer.transform.rotation = this.startLine.rotation;
        }
    }

    private Vector3 GetPosition(int i)
    {
        var xCoefficient = i % 2f == 0f ? -1 : 1;
        var xOffset = this.startLine.right * this.startPositioning.x * xCoefficient;

        var yCoefficient = (-1 * (i / 2)) - 1;
        var yOffset = this.startLine.forward * this.startPositioning.y * yCoefficient;

        var zOffset = this.startLine.up * this.startPositioning.z;

        return this.startLine.position + xOffset + yOffset + zOffset;
    }

    private IEnumerator StartRaceLocal()
    {
        StartCoroutine(this.PositionRacers());

        // Intro
        GameController.PlaySound(this.introTune);

        foreach (var cam in this.introCameras)
            yield return cam.PlayDolly();

        // Ready
        for (int i = 0; i < this.raceTextControllers.Count; i++)
        {
            var controller = this.raceTextControllers[i];
            if (i + 1 >= this.raceTextControllers.Count)
                yield return controller.ShowRaceReady();
            else
                StartCoroutine(controller.ShowRaceReady());
        }

        // Start race
        foreach (var racer in this.racers)
            racer.RaceStart();

        foreach (var controller in this.raceTextControllers)
            StartCoroutine(controller.ShowRaceStart());

        foreach (var particles in this.startParticles)
            particles.Play();

        yield return GameController.PlaySoundAsCoroutine(this.startShot);

        // Start music
        GameController.PlayMusic();
    }

    private IEnumerator EndRace()
    {
        if (this.raceComplete)
            yield break;

        this.raceComplete = true;

        yield return new WaitForSeconds(5);

        GameController.LoadScene(Scenes.Menu);
    }

    private void UpdatePositions()
    {
        var orderedRacers = racers
            .OrderByDescending(r => r.LapsCompleted)
            .ThenByDescending(r => r.NextCheckpoint.Index == 0)
            .ThenByDescending(r => r.NextCheckpoint.Index)
            .ThenBy(r => this.GetDistanceToCheckpoint(r, r.NextCheckpoint))
            .ToList();

        this.racePositions.Clear();

        for (int i = 0; i < orderedRacers.Count; i++)
            this.racePositions[orderedRacers[i]] = i + 1;
    }

    public static void LogFinish(CarMovement car, out int finalPosition) => _instance.LogFinishLocal(car, out finalPosition);

    public void LogFinishLocal(CarMovement car, out int finalPosition)
    {
        this.finishedRacers.Add(car);
        Debug.Log($"{car.name} finished");

        foreach (var particles in this.finishParticles)
            particles.Play();

        finalPosition = this.finishedRacers.IndexOf(car) + 1;
        if (!car.IsAi)
        {
            this.finishedPlayers++;
            GameController.PlaySound(this.finishSound);
        }

        if (this.finishedPlayers == this.totalPlayerCount)
            StartCoroutine(this.EndRace());
    }

    public static bool TryGetPosition(CarMovement car, out int position)
    {
        position = -1;
        if (!_instance) return false;

        return _instance.TryGetPositionLocal(car, out position);
    }

    private bool TryGetPositionLocal(CarMovement car, out int position)
    {
        if (this.finishedRacers.Contains(car))
        {
            position = this.finishedRacers.IndexOf(car) + 1;
            return true;
        }

        return this.racePositions.TryGetValue(car, out position);
    }

    public static bool TryGetRacer(int position, out CarMovement car) => _instance.TryGetRacerLocal(position, out car);

    private bool TryGetRacerLocal(int position, out CarMovement car)
    {
        car = default;
        if (this.racePositions.Count < this.totalRacerCount)
            return false;

        var result = false;
        foreach (var kvp in this.racePositions)
        {
            if (kvp.Value == position)
            {
                car = kvp.Key;
                result = true;
                break;
            }
        }

        return result;
    }

    private float GetDistanceToCheckpoint(CarMovement car, Checkpoint checkpoint) =>
        Mathf.Min(
            Vector3.Distance(car.transform.position, checkpoint.MinimumInnerBoundary),
            Vector3.Distance(car.transform.position, checkpoint.MaximumInnerBoundary)
        );

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < 8; i++)
        {
            var position = this.GetPosition(i);
            Gizmos.DrawSphere(position, 2);
        }
    }
}
