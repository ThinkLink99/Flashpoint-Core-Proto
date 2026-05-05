public abstract class BaseGameState : IState
{
    protected GameManager gameManager;

    protected BaseGameState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void Update()
    {
    }

    public override string ToString()
    {
        return this.GetType().Name;
    }
}
