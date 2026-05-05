using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
// Director should handle events and game flow / game states 
public class Director : MonoBehaviour
{
    [SerializeField] private GameInfoSO gameInfoSO;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerBuilder builder;
    [SerializeField] private ModelSpawner spawner;
    [SerializeField] private MapBuilder mapBuilder;

    [SerializeField] List<PlayerController> playerControllers;

    [Header("Game Events")]
    [SerializeField] private GameEvent onGameStart;
    [SerializeField] private GameEvent onMapSelected;
    [SerializeField] private GameEvent onPlayersCreated;
    [SerializeField] private GameEvent onAllPlayersDeployed;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private ModelInformationController playerUI;

    [Header("Debug State Change Flags")]
    [SerializeField] private string currentStateName = string.Empty;
    [SerializeField] private bool startApp = false;
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private bool mapSelected = false;
    [SerializeField] private bool mapBuilt = false;
    [SerializeField] private bool playersDeployed = false;
    [SerializeField] private int round = 0;

    private StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine();

        var appStartState = new AppStartState(gameManager, loadingScreen, mapBuilder, builder, spawner.configurations.ToArray(), gameInfoSO.PlayerOptions, onGameStart);
        var deploymentState = new DeploymentRoundState(gameManager, onAllPlayersDeployed: onAllPlayersDeployed);
        var roundState = new GameRoundState(gameManager, modelInformationController: playerUI);

        At(appStartState,  
           transitionTo: deploymentState, 
           when: new FuncPredicate(() => gameStarted));
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
        // read our return data from the app start state and assign it to our director so we can pass it to other states as needed
        if (data is AppStartReturnData ret)
        {
            gameManager.currentMap = ret.Map;
            gameManager.players = ret.Players;
        }

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
    public void OnPlayersCreated(Component sender, object data)
    {
        // temporarily assign UI to player here. Need a more elegant solution in the future
        if (data is List<PlayerController> players)
        {
            playerControllers = players;
            playerUI.playerController = players[0];
        }
    }
    public void OnAllPlayersDeployed (Component sender, object data)
    {
        Debug.Log("All Players Deployed!");
        playersDeployed = true;
    }
    #endregion
}
