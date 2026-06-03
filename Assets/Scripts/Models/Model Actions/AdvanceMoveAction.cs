using UnityEngine;

public class AdvanceMoveAction : MoveAction
{
    public override int Cost => 1;
    public AdvanceMoveAction(MovementPlanner planner) : base(planner) { }
}