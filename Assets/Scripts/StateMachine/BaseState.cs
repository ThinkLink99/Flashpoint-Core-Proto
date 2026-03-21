using UnityEngine;
public abstract class BaseState : IState
{
    protected PlayerController playerController;

    protected BaseState(PlayerController playerController)
    {
        this.playerController = playerController;
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
}