using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public record AppStartReturnData
{
    public Map Map { get; set; }
    public List<PlayerController> Players { get; set; }
}
public class AppStartState : BaseGameState
{
    private GameObject loadingScreen;
    private readonly GameEvent onGameStart;
    private readonly MapBuilder mapBuilder;
    private readonly PlayerBuilder playerBuilder;
    private readonly IEnumerable<PlayerOptions> playerOptions;

    private List<PlayerController> playersCreated;
    private Map currentMap;

    private ModelConfiguration[] testModelsToSpawn; // Fireteams should be handled by a json object or ScriptableObject. this should not be permanent


    public AppStartState(GameManager gameManager, GameObject loadingScreen, MapBuilder mapBuilder, PlayerBuilder playerBuilder, ModelConfiguration[] testModelsToSpawn, IEnumerable<PlayerOptions> playerOptions, GameEvent onGameStart) : base(gameManager)
    {
        this.loadingScreen = loadingScreen;
        this.onGameStart = onGameStart;
        this.mapBuilder = mapBuilder;
        this.playerBuilder = playerBuilder;
        this.testModelsToSpawn = testModelsToSpawn;
        this.playerOptions = playerOptions;
    }

    public override void OnEnter()
    {
        // show loading screen
        loadingScreen.SetActive(true);
    }
    public override void Update()
    {
        // do any loading that needs done here
        currentMap = mapBuilder.Start()
          .RaiseMapCreatingEvent()
          //.SpawnGroundPlane()
          .SpawnGridLines()
          .SpawnTerrain()
          .DrawDeploymentZones()
          .Build();

        playersCreated = new List<PlayerController>();
        foreach (var playerOption in playerOptions)
        {
            var builder = playerBuilder.Start()
                                  .AddFireteam(testModelsToSpawn)
                                  .SetName(playerOption.playerName)
                                  .SetTeam(playerOption.teamId);
            if (playerOption.isHuman)
                builder.IsHuman();
            else builder.IsHuman(false);

            PlayerController player = builder.Build();
            if (player.isHumanControlled) player.IsLocalPlayer = true;
            playersCreated.Add(player);
        }

        // Raise the game start event to trigger the next state transition
        onGameStart.Raise(null, new AppStartReturnData { Map = currentMap, Players = playersCreated });
    }
    public override void OnExit()
    {
        // hide loading screen
        loadingScreen.SetActive(false);
    }
}