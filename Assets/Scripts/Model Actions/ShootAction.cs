using System.Collections;

public class ShootAction : IGameAction
{
    public int Cost => 1;

    public bool CanExecute(GameActionContext ctx)
    {
        if (ctx == null) return false;

        if (ctx.SourceModel.ActionController.RemainingAP >= Cost) return true;
        // check that model has line of sight to target model 
        // will need future checks for keywords that force the weapon into a long shoot action
        else return false;
    }

    public IEnumerator Execute(GameActionContext ctx)
    {
        throw new System.NotImplementedException();
    }
}