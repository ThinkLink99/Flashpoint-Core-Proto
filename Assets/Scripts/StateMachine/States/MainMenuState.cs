using UnityEngine;

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
