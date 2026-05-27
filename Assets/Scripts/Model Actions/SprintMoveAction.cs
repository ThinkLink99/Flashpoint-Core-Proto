public class SprintMoveAction : MoveAction
{
    public override int Cost => 2;
    public SprintMoveAction(MovementPlanner planner) : base(planner) { }
}
