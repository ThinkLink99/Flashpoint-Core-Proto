using UnityEngine;
public class ModelSelectedState : BaseState
{
    public ModelSelectedState(PlayerController playerController) : base(playerController) { }

    public override void OnEnter()
    {
        Debug.Log("Entering Model Selected");
    }
    public override void Update()
    {

    }
}

public class ModelActivatedState : BaseState
{
    public ModelActivatedState(PlayerController playerController) : base(playerController) { }
    public override void OnEnter()
    {
        Debug.Log("Entering Model Activated");
    }
    public override void Update()
    {

    }
}
