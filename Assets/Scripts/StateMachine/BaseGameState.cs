public abstract class BaseGameState : IState
{
    protected Tabletop tabletop;

    protected BaseGameState(Tabletop tabletop)
    {
        this.tabletop = tabletop;
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
