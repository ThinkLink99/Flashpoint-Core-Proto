using System.Collections;
using UnityEngine; 

public class AdvanceAction : IGameAction
{
    public int Cost => 1;
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

        Vector3 clamped = planner.ClampPointToRange(ctx.OriginCube, ctx.SelectedPoint, ctx.SourceModel.ModelConfiguration.unitAdvanceSpeed, modelYOffset);

        //var point = ctx.SelectedPoint + (ctx.SourceModel.transform.up * 20);
        ctx.SourceModel.MoveModelToPoint(clamped);
        ctx.SourceModel.ActionController.HasMoved = true;

        yield return null;
    }
}
