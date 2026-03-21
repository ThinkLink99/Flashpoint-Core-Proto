using UnityEngine;

public class ModelActionController : MonoBehaviour
{
    public const int ACTIVATION_STARTING_AP = 2;

    [Header("Activation Details")]
    [SerializeField] private int remainingAP = 0;
    [SerializeField] private bool hasActivated = false;
    [SerializeField] private bool isActivated = false; // this is GOING to need changed to be handled globally.
    [SerializeField] private Model unit;               // or atleast have an event fire that updates other models if a user tries to activate something else before an action has taken place.
    [SerializeField] private GameEvent onActivationEnded;

    [Header ("ActionsTaken")]
    [SerializeField] private bool hasMoved = false;
    [SerializeField] private bool hasShot = false;
    [SerializeField] private bool hasAssaulted = false;
    [SerializeField] private bool hasUsedItem = false;

    private void Start()
    {
        unit = GetComponent<Model>();
    }

    public int RemainingAP { get => remainingAP; private set => remainingAP = value; }
    public bool HasActivated { get => hasActivated; set => hasActivated = value; }
    public bool IsActivated { get => isActivated; set => isActivated = value; }
    public bool HasMoved { get => hasMoved; set => hasMoved = value; }
    public bool HasShot { get => hasShot; set => hasShot = value; }
    public bool HasAssaulted { get => hasAssaulted; set => hasAssaulted = value; }
    public bool HasUsedItem { get => hasUsedItem; set => hasUsedItem = value; }

    public void BeginActivation()
    {
        RemainingAP = ACTIVATION_STARTING_AP;
        IsActivated = true;
    }
    public void EndActivation()
    {
        RemainingAP = 0;
        HasActivated = true;
        IsActivated = false;

        OnActivationComplete();
    }

    public bool TryPerformAction(IModelAction action, ModelActionContext ctx)
    {
        if (action.Cost > RemainingAP) return false;
        if (!action.CanExecute(ctx)) return false;
        RemainingAP -= action.Cost;
        // start the action coroutine on this MonoBehaviour so the action can perform animations/movement
        StartCoroutine(action.Execute(ctx));
        if (RemainingAP == 0) OnActivationComplete();
        return true;
    }

    private void OnActivationComplete()
    {
        // notify turn manager, disable input, fire events
        onActivationEnded?.Raise(this, unit);
    }
}