using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
// Director should handle events and game flow / game states 
public class Director : MonoBehaviour
{
    [SerializeField] private Tabletop tabletop;
    [SerializeField] private ModelSpawner spawner;
    [SerializeField] private MapBuilder mapBuilder;

    [Header("Game Events")]
    [SerializeField] private GameEvent onGameStart;
    [SerializeField] private GameEvent onMapSelected;
    [SerializeField] private GameEvent onPlayersCreated;
    [SerializeField] private GameEvent onAllPlayersDeployed;

    [SerializeField] private GameObject mainMenuPanel;

    private StateMachine stateMachine;

    [Header("Debug State Change Flags")]
    [SerializeField] private string currentStateName = string.Empty;
    [SerializeField] private bool startApp = false;
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private bool mapSelected = false;
    [SerializeField] private bool mapBuilt = false;
    [SerializeField] private bool playersDeployed = false;
    [SerializeField] private int round = 0;

    private void Awake()
    {
        stateMachine = new StateMachine();

        var appStartState = new AppStartState(tabletop);
        var mainMenuState = new MainMenuState(tabletop, onGameStart: onGameStart,
                                                        mainMenu: mainMenuPanel);
        var mapSelectState = new MapSelectState(tabletop, onMapSelected: onMapSelected);
        var mapBuildState = new MapBuildState (tabletop, mapBuilder);
        var playerCreationState = new PlayerCreationState(tabletop, spawner, onPlayersCreated: onPlayersCreated, testModelsToSpawn: spawner.configurations.ToArray());
        var deploymentState = new DeploymentRoundState(tabletop, onAllPlayersDeployed: onAllPlayersDeployed);
        var roundState = new GameRoundState(tabletop);

        At(appStartState, 
           transitionTo: mainMenuState, 
           when: new FuncPredicate(() => startApp));
        At(mainMenuState,
           transitionTo: mapSelectState,
           when: new FuncPredicate(() => gameStarted));
        At(mapSelectState,
           transitionTo: mapBuildState,
           when: new FuncPredicate(() => mapSelected));
        At(mapBuildState,
           transitionTo: playerCreationState,
           when: new FuncPredicate(() => mapBuilt));
        At(playerCreationState,
           transitionTo: deploymentState,
           when: new FuncPredicate(() => tabletop.players.Count > 0));
        At(deploymentState,
           transitionTo: roundState,
           when: new FuncPredicate(() => playersDeployed));

        stateMachine.SetState(appStartState);
    }

    void At(IState from, IState transitionTo, IPredicate when) => stateMachine.AddTransition(from, transitionTo, when);
    void FromAny(IState transitionTo, IPredicate when) => stateMachine.AddAnyTransition(transitionTo, when);

    private void Start()
    {
        
    }
    private void Update()
    {
        stateMachine.Update();
        currentStateName = stateMachine.CurrentState.State.ToString();
    }
    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    #region Event Listeners
    public void OnGameStart (Component sender, object data)
    {
        // change our state to select map (noop, just use default map for now)
        gameStarted = true;
    }
    public void OnMapSelected(Component sender, object data)
    {
        mapSelected = true;
    }
    public void OnMapCreated(Component sender, object data)
    {
        mapBuilt = true;
    }
    public void OnAllPlayersDeployed (Component sender, object data)
    {
        playersDeployed = true;
    }
    #endregion
}
