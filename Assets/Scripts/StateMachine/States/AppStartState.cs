using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppStartState : BaseGameState
{
    private GameObject loadingScreen;
    private readonly GameEvent onGameStart;
    private readonly MapBuilder mapBuilder;

    private readonly PlayerBuilder playerBuilder;
    private List<PlayerController> playersCreated;
    private ModelConfiguration[] testModelsToSpawn; // Fireteams should be handled by a json object or ScriptableObject. this should not be permanent

    public AppStartState(Tabletop tabletop, GameObject loadingScreen, MapBuilder mapBuilder, PlayerBuilder playerBuilder, ModelConfiguration[] testModelsToSpawn, GameEvent onGameStart) : base(tabletop)
    {
        this.loadingScreen = loadingScreen;
        this.onGameStart = onGameStart;
        this.mapBuilder = mapBuilder;
        this.playerBuilder = playerBuilder;
        this.testModelsToSpawn = testModelsToSpawn;
    }

    public override void OnEnter()
    {
        // show loading screen
        loadingScreen.SetActive(true);
    }
    public override void Update()
    {
        // do any loading that needs done here
        Game.Instance.CurrentMap = mapBuilder.Start()
          .RaiseMapCreatingEvent()
          .SpawnGroundPlane()
          .SpawnGridLines()
          .SpawnTerrain()
          .DrawDeploymentZones()
          .Build();

        playersCreated = new List<PlayerController>();
        var playerOptions = Game.Instance.GameInfo.PlayerOptions;
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
            playersCreated.Add(player);
        }

        Game.Instance.Players = playersCreated;

        // Raise the game start event to trigger the next state transition
        Game.Instance.RaiseGameStartEvent (null, null);
    }
    public override void OnExit()
    {
        // hide loading screen
        loadingScreen.SetActive(false);
    }
}