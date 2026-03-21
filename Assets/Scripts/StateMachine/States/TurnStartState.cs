using Assets.Scripts;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerIdleState : BaseState
{
    public PlayerIdleState(PlayerController playerController) : base(playerController) { }
}
public class TurnStartState : BaseState
{
    public TurnStartState(PlayerController playerController) : base(playerController) { }

    public override void OnEnter()
    {
        playerController.BeginTurn();
    }
    public override void Update()
    {
    }
}

public class AppStartState : BaseGameState
{
    public AppStartState(Tabletop tabletop) : base(tabletop) { }

    public override void Update()
    {
        // do any loading that needs done here
    }
}
public class MainMenuState : BaseGameState
{
    public GameEvent onGameStart;

    [SerializeField] private GameObject mainMenu;

    public MainMenuState(Tabletop tabletop, GameEvent onGameStart, GameObject mainMenu) : base(tabletop)
    {
        this.onGameStart = onGameStart;
        this.mainMenu = mainMenu;
    }

    public override void Update()
    {
        // do any loading that needs done here
    }

    public override void OnEnter()
    {
        mainMenu.SetActive(true);
    }
    public override void OnExit()
    {
        mainMenu.SetActive(false);
    }
}
public class MapSelectState : BaseGameState
{
    private GameEvent onMapSelected;

    public MapSelectState(Tabletop tabletop, GameEvent onMapSelected) : base(tabletop)
    {
        this.onMapSelected = onMapSelected;
    }

    public override void Update()
    {
        // do any loading that needs done here
        onMapSelected?.Raise(null, null);
    }
}
public class MapBuildState : BaseGameState
{
    private readonly MapBuilder mapBuilder;

    public MapBuildState(Tabletop tabletop, MapBuilder mapBuilder) : base(tabletop)
    {
        this.mapBuilder = mapBuilder;
    }

    public override void Update()
    {
        // do any loading that needs done here
        mapBuilder.BuildMap()
                  .RaiseMapCreatingEvent()
                  .SpawnGroundPlane()
                  .SpawnGridLines()
                  .SpawnTerrain()
                  .DrawDeploymentZones()
                  .RaiseMapCreatedEvent();
    }
}
public class PlayerCreationState : BaseGameState
{
    private readonly PlayerBuilder playerBuilder;
    private readonly GameEvent onPlayersCreated;
    private readonly List<PlayerController> playersCreated;
    private ModelConfiguration[] testModelsToSpawn; // Fireteams should be handled by a json object or ScriptableObject. this should not be permanent

    public PlayerCreationState(Tabletop tabletop, ModelSpawner spawner, GameEvent onPlayersCreated, params ModelConfiguration[] testModelsToSpawn) : base(tabletop)
    {
        playerBuilder = new PlayerBuilder(tabletop.transform, spawner);
        this.onPlayersCreated = onPlayersCreated;

        playersCreated = new List<PlayerController>();
        this.testModelsToSpawn = testModelsToSpawn;
    }

    public override void OnEnter()
    {
        // for now, create a temporary player here. A menu of some kind should be made later.
        PlayerController player1 = playerBuilder.Start()
                                  .SetTeam(TeamId.Red)
                                  .AddFireteam(testModelsToSpawn)
                                  .Build();
        playersCreated.Add(player1);

        PlayerController player2 = playerBuilder.Start()
                          .SetTeam(TeamId.Blue)
                          .AddFireteam(testModelsToSpawn)
                          .Build();
        playersCreated.Add(player2);

        onPlayersCreated?.Raise(null, playersCreated);
    }
    public override void Update()
    {
        // do any loading that needs done here
    }
}
public class DeploymentRoundState : BaseGameState
{
    private List<PlayerController> players;
    private bool[] hasDeplopyed;
    private Map currentMap;

    private GameEvent onAllPlayersDeployed;

    public DeploymentRoundState(Tabletop tabletop, GameEvent onAllPlayersDeployed) : base(tabletop)
    {
        this.onAllPlayersDeployed = onAllPlayersDeployed;
    }

    public override void OnEnter()
    {
        players = tabletop.players;
        currentMap = tabletop.currentMap;
        hasDeplopyed = new bool[players.Count];
    }
    public override void Update()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log($"Has Deployed: {hasDeplopyed[i]}");
            if (hasDeplopyed[i]) continue;

            // otherwise we need to let the player deploy.
            // For now, just spawn the models in each square of the deployment zone like before
            // then mark the player deployed

            var playerDeploying = players[i];
            var spawner = playerDeploying.modelSpawner;

            // Initialize player state, such as setting up the fireteam, resetting any turn-specific data, etc.
            //if (showDebugLogs) Debug.Log($"Player {name} has started the game with fireteam of {fireteam.Models.Count} models.");

            // temporarily loop through units and ground level deployment cubes and spawn a model of a unit in each cube
            var zone = currentMap.GetZoneForTeam(playerDeploying.team);
            //if (showDebugLogs) Debug.Log($"Zones for {team.ToString()}: {zone.squares.Count}");
            for (int z = 0; z < zone.squares.Count; z++)
            {
                if (playerDeploying.fireteam.Models[z] != null)
                {
                    var worldPos = new Vector3(zone.squares[z].x, 1, zone.squares[z].y) * currentMap.CubeSize;
                    var spawnedModel = spawner.SpawnForPlayer(playerDeploying.fireteam.Models[z].name, playerDeploying, worldPos);
                    playerDeploying.spawnedModels.Add(spawnedModel);
                }
            }

            hasDeplopyed[i] = true;
            if (i == players.Count - 1) 
                // raise an event to say we are done deploying
                onAllPlayersDeployed?.Raise(null, null);
        }
    }
}
public class GameRoundState : BaseGameState
{
    private List<PlayerController> players;
    private int playerTurnIndex = -1;

    public GameRoundState(Tabletop tabletop) : base(tabletop)
    {
    }

    public override void OnEnter()
    {
        players = tabletop.players;
    }
    public override void Update()
    {
        // On Update we need to check whose turn it is and allow that players state machine drive their UI and state. 
        // 

    }
}