using System.Collections;
using UnityEngine;

public class AdvanceAction : IModelAction
{
    public int Cost => 1;
    public bool CanExecute(ModelActionContext ctx)
    {
        if (ctx == null) return false;

        if (ctx.SourceModel.ActionController.RemainingAP >= Cost) return true;
        else return false;
    }
    public IEnumerator Execute(ModelActionContext ctx)
    {
        if (ctx == null) yield break;

        var planner = new MovementPlanner(ctx);
        Debug.Log(ctx.Map.MapName);

        // compute model's vertical offset relative to origin cube center so placement keeps the same visual height
        float modelYOffset = 0f;
        if (ctx.OriginCube != null)
        {
            modelYOffset = ctx.SourceModel.transform.position.y - ctx.OriginCube.worldPosition.y;
        }

        Vector3 clamped = planner.ClampPointToRange(ctx.OriginCube, ctx.SelectedPoint, ctx.SourceModel.unit.unitAdvanceSpeed, modelYOffset);

        //var point = ctx.SelectedPoint + (ctx.SourceModel.transform.up * 20);
        ctx.SourceModel.MoveModelToPoint(clamped);

        yield return null;
    }
}
