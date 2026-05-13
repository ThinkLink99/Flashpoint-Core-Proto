using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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

    private void Awake() { }
    private void Start()
    {
        unit = GetComponent<Model>();
    }
    private void Update() { }

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

    private void OnActivationComplete()
    {
        // notify turn manager, disable input, fire events
        onActivationEnded?.Raise(this, unit);
    }
}