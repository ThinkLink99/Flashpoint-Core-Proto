using UnityEngine;

public class UnitActionController : MonoBehaviour
{
    public int RemainingAP { get; private set; } = 0;
    public void BeginActivation() => RemainingAP = 2;
    public void EndActivation() => RemainingAP = 0;

    public bool TryPerformAction(IUnitAction action, Model unit)
    {
        if (action.Cost > RemainingAP) return false;
        if (!action.CanExecute(unit)) return false;
        RemainingAP -= action.Cost;
        _ = action.Execute(unit); // handle async/coroutines properly
        if (RemainingAP == 0) OnActivationComplete(unit);
        return true;
    }

    private void OnActivationComplete(Model unit)
    {
        // notify turn manager, disable input, fire events
    }
}