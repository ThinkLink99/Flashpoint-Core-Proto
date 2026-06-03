using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public GameActionContext Context { get; private set; }

    private MapBuilder mapBuilder;
    private PlayerBuilder playerBuilder;
    public Camera mainCamera;
    [SerializeField] private TabletopCamera tabletopCameraController;

    [Header("Players")]
    [SerializeField] private PlayerOptions[] playerOptions;
    [SerializeField] public List<PlayerController> players;
    [SerializeField] private ModelConfiguration[] testModelsToSpawn;

    private TurnManager turnManager;
    public Map currentMap;


    private void Awake()
    {
        Context = new GameActionContext();

        playerBuilder = GetComponent<PlayerBuilder>();
        mapBuilder = FindAnyObjectByType<MapBuilder>();
    }
    // World-level events raised by the authoritative GameManager. Other systems (UI, network)
    // can subscribe to these to be notified when the game state changes.
    public event System.EventHandler<ModelSelectedEventArgs> OnWorldModelSelected;
    public event System.EventHandler<DestinationSelectedEventArgs> OnWorldDestinationSelected;
    public event System.EventHandler<TargetSelectedEventArgs> OnWorldTargetSelected;
    public event System.EventHandler<ModelMovedEventArgs> OnModelMoved;
    public event System.EventHandler<ModelShotEventArgs> OnModelShot;
    public event System.EventHandler<PlayersCreatedEventArgs> OnPlayersCreated;

    // Request APIs: clients send intent through Request* methods. GameManager validates
    // and if accepted applies the change and raises world events so all clients see
    // the authoritative result.
    public void RequestSelectModel(Model model, PlayerController requester)
    {
        // basic validation: only allow if requester has command permission (or requester null for server)
        if (requester != null && !requester.AllowCommands)
        {
            Debug.Log($"Select request from {requester.name} rejected: no command permission");
            return;
        }

        // apply selection (mutates authoritative context)
        SelectModel(model);

        // raise world event for listeners
        OnWorldModelSelected?.Invoke(this, new ModelSelectedEventArgs(model, requester));
    }
    public void RequestSelectDestination(Vector3 destination, PlayerController requester)
    {
        if (requester != null && !requester.AllowCommands)
        {
            Debug.Log($"Select destination request from {requester.name} rejected: no command permission");
            return;
        }

        SelectDestination(destination);
        OnWorldDestinationSelected?.Invoke(this, new DestinationSelectedEventArgs(destination, requester));
    }
    public void RequestSelectTarget(Model target, PlayerController requester)
    {
        if (requester != null && !requester.AllowCommands)
        {
            Debug.Log($"Select target request from {requester.name} rejected: no command permission");
            return;
        }

        SelectTarget(target);
        OnWorldTargetSelected?.Invoke(this, new TargetSelectedEventArgs(target, requester));
    }
    public void RequestMove(Model model, Vector3 destination, PlayerController requester)
    {
        if (requester != null && !requester.AllowCommands)
        {
            Debug.Log($"Move request from {requester.name} rejected: no command permission");
            return;
        }

        // Authoritative: select source and destination
        Context.SourceModel = model;
        Context.OriginCube = model.CurrentCube;
        Context.SelectedPoint = destination;

        IGameAction action = null;
        // we need to determine if the move is within advance range or sprint range to call the right move function
        if (requester.MovePlanner.IsSprintMove (Context.OriginCube, Context.SelectedPoint))
        {
            // Attempt to execute SprintAction
            action = new SprintMoveAction(new MovementPlanner(Context));
            if (!action.CanExecute(Context))
            {
                Debug.Log("Sprint action cannot execute");
                return;
            }
        }
        else
        {
            // Attempt to execute AdvanceAction
            action = new AdvanceMoveAction(new MovementPlanner(Context));
            if (!action.CanExecute(Context))
            {
                Debug.Log("Advance action cannot execute");
                return;
            }
        }


        // Start coroutine to perform the action and raise event when done
        StartCoroutine(ExecuteActionCoroutine(action, () =>
        {
            OnModelMoved?.Invoke(this, new ModelMovedEventArgs(model, destination, requester));
        }));
    }
    public void RequestShoot(Model source, Model target, Weapon weapon, PlayerController requester)
    {
        if (requester != null && !requester.AllowCommands)
        {
            Debug.Log($"Shoot request from {requester.name} rejected: no command permission");
            return;
        }

        // Authoritative: select source and target
        Context.SourceModel = source;
        Context.TargetModel = target;
        Context.WeaponUsed = weapon;

        var action = new ShootAction();
        if (!action.CanExecute(Context))
        {
            Debug.Log("Shoot action cannot execute");
            return;
        }

        StartCoroutine(ExecuteActionCoroutine(action, () =>
        {
            OnModelShot?.Invoke(this, new ModelShotEventArgs(source, target, requester));
        }));
    }
    public void RequestShoot(PlayerController requester)
    {
        if (requester != null && !requester.AllowCommands)
        {
            Debug.Log($"Shoot request from {requester.name} rejected: no command permission");
            return;
        }

        var action = new ShootAction();
        if (!action.CanExecute(Context))
        {
            Debug.Log("Shoot action cannot execute");
            return;
        }

        StartCoroutine(ExecuteActionCoroutine(action, () =>
        {
            OnModelShot?.Invoke(this, new ModelShotEventArgs(Context.SourceModel, Context.TargetModel, requester));
        }));
    }

 
    private System.Collections.IEnumerator ExecuteActionCoroutine(IGameAction action, System.Action onComplete)
    {
        yield return action.Execute(Context);
        onComplete?.Invoke();
    }

    void Start()
    {
        if (tabletopCameraController == null) mainCamera.TryGetComponent<TabletopCamera>(out tabletopCameraController);

        BuildMap();
        CreatePlayerObjects(this.playerOptions);
        DeployModels();

        players[0].IsLocalPlayer = true;
        StartTurn(players[0]);
    }
    void Update()
    {
    }

    private void FixedUpdate()
    {
        currentMap.MarkCubesWithTerrainBelow(); // this isn't killing frames yet but we need to better poll this. Checking adjacent cubes when a model moves into a new cube would be good.
    }

    public void BuildMap ()
    {
        currentMap = mapBuilder.Start()
          .RaiseMapCreatingEvent()
          //.SpawnGroundPlane()
          .SpawnGridLines()
          .SpawnTerrain()
          .DrawDeploymentZones()
          .Build();

    }
    public void CreatePlayerObjects (params PlayerOptions[] playerOptions)
    {
        var playersCreated = new List<PlayerController>();
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

        players.AddRange(playersCreated);
        OnPlayersCreated?.Invoke(this, new PlayersCreatedEventArgs(playersCreated.ToArray()));
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
                    var worldPos = new Vector3(zone.squares[z].x, 2f, zone.squares[z].y) * currentMap.CubeSize;
                    var spawnedModel = spawner.SpawnForPlayer(playerDeploying.fireteam.Models[z].name, playerDeploying, worldPos);
                    playerDeploying.spawnedModels.Add(spawnedModel);
                }
            }

            //Time.timeScale = 0f; // pause the game to allow for deployment

            //if (i == players.Count - 1)
            // raise an event to say we are done deploying
            //onAllPlayersDeployed.Raise(null, null);
        }
    }

    private void StartRound ()
    {

    }

    public void StartTurn (PlayerController player)
    {
        Context.InitiatingPlayer = player;
        Context.SourceModel = null;
        Context.Map = currentMap;

        // Grant/revoke command permission:
        foreach (var p in players)
        {
            p.AllowCommands = false;
        }
        player.AllowCommands = player.isHumanControlled; // true for human player; false for AI

        Debug.Log ($"Starting turn for player: {player.name}");

        // If player is AI, find its AI controller and start it
        if (!player.isHumanControlled)
        {
            var ai = player.GetComponent<AIPlayerController>();
            if (ai != null)
            {
                ai.BeginTurn();
            }
        }
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