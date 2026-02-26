using System;
using System.Collections;
using System.Threading.Tasks;

public class AdvanceAction : IUnitAction
{
    public int Cost => 1;
    public bool CanExecute(Model unit)
    {
        if (unit == null) return false;

        if (unit.ActionController.RemainingAP >= Cost) return true;
        else return false;
    }
    public IEnumerator Execute(Model unit)
    {
        if (unit == null) yield break;
        var tabletop = unit.tabletop;   
        if (tabletop == null) yield break;

        var point = tabletop.SelectedPoint + (unit.transform.up * 20);
        unit.MoveModelToPoint(point);
        yield return null;
    }
}
