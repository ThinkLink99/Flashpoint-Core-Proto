using UnityEngine;

[RequireComponent(typeof(Model))]
public class ModelActionController : MonoBehaviour
{
    [Header("Activation Details")]
    [SerializeField] private int remainingAP = 0;
    [SerializeField] private bool hasActivated = false;
    [SerializeField] private bool isActivated = false; // this is GOING to need changed to be handled globally.
                                                       // or atleast have an event fire that updates other models if a user tries to activate something else before an action has taken place.

    public int RemainingAP { get => remainingAP; private set => remainingAP = value; }
    public bool HasActivated { get => hasActivated; set => hasActivated = value; }
    public bool IsActivated { get => isActivated; set => isActivated = value; }

    public void BeginActivation()
    {
        RemainingAP = 2;
        IsActivated = true;
    }
    public void EndActivation()
    {
        RemainingAP = 0;
        HasActivated = true;
        IsActivated = false;

        OnActivationComplete(this.GetComponent<Model>());
    }

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