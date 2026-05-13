using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public GameActionContext Context { get; private set; }

    public Camera mainCamera;
    [SerializeField] private TabletopCamera tabletopCameraController;

    [Header("Players")]
    [SerializeField] public List<PlayerController> players;

    public Map currentMap;

    private void Awake()
    {
        Context = new GameActionContext();
    }
    void Start()
    {
        if (tabletopCameraController == null) mainCamera.TryGetComponent<TabletopCamera>(out tabletopCameraController);
    }
    void Update()
    {

    }

    public void OnMapCreated(Component sender, object data)
    {
        if (data is Map map)
        {
            currentMap = map;
        }
    }
    public void OnPlayersCreated(Component sender, object data)
    {
        Debug.Log("Hit");
        if (data is List<PlayerController> players)
        {
            this.players = players;
        }
    }
    private void DeployModels()
    {
        for (int i = 0; i < players.Count; i++)
        {
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

            //if (i == players.Count - 1)
            // raise an event to say we are done deploying
            //onAllPlayersDeployed.Raise(null, null);
        }
    }

    public void StartTurn (PlayerController player)
    {
        Context.InitiatingPlayer = player;
        Context.SourceModel = null;
        Context.Map = currentMap;

        Debug.Log ($"Starting turn for player: {player.name}");
    }
    public void SelectModel (Model model)
    {
        Context.SourceModel = model;
        Context.OriginCube = model.CurrentCube;

        Debug.Log ($"Selected model: {model.name}");
    }
    public void SelectDestination (Vector3 destination)
    {
        Context.SelectedPoint = destination;

        Debug.Log ($"Selected destination: {destination}");
    }
    public void SelectTarget (Model target)
    {
        Context.TargetModel = target;

        Debug.Log ($"Selected target: {target.name}");
    }
    public void EndTurn (PlayerController player)
    {
    }

    public bool TryExecuteAction (IGameAction action)
    {
        if (!action.CanExecute(Context)) return false;

        // start the action coroutine on this MonoBehaviour so the action can perform animations/movement
        StartCoroutine(action.Execute(Context));
        return true;
    }
}