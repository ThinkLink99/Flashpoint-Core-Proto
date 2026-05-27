using System.Collections;

public abstract class MoveAction : IGameAction
{
    private float height_offset_multiplier = 1f; // multiply the models Up vector by this amount and offset the selected point by that much
    private readonly MovementPlanner planner = null;

    public MoveAction(MovementPlanner planner)
    {
        this.planner = planner;
    }

    public virtual int Cost => 0;
    public bool CanExecute(GameActionContext ctx)
    {
        if (ctx == null) return false;

        if (ctx.SourceModel.ActionController.RemainingAP >= Cost) return true;
        else if (ctx.SourceModel.ActionController.HasMoved) return false;
        else return false;
    }
    public IEnumerator Execute(GameActionContext ctx)
    {
        if (ctx == null) yield break;

        var planner = new MovementPlanner(ctx);

        // compute model's vertical offset relative to origin cube center so placement keeps the same visual height
        float modelYOffset = 0f;
        if (ctx.OriginCube != null)
        {
            modelYOffset = ctx.SourceModel.transform.position.y - ctx.OriginCube.worldPosition.y;
        }

        var point = ctx.SelectedPoint + (ctx.SourceModel.transform.up * height_offset_multiplier);
        ctx.SourceModel.MoveModelToPoint(point);
        ctx.SourceModel.ActionController.HasMoved = true;

        yield return null;
    }
}