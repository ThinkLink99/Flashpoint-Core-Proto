using System.Collections.Generic;
using UnityEngine;

public enum TurnState
{
    SelectingUnit,
    UnitSelected,
    MovingUnit,
    ChoosingRangedAttackTarget,
    ResolvingAction
}
public class GameActionContext
{
    public TurnState TurnState;
    public Map Map { get; set; }
    public Vector3 SelectedPoint { get; set; }
    public Cube OriginCube { get; set;  }
    public Model SourceModel { get; set; }
    public Model TargetModel { get; set; }
    public PlayerController InitiatingPlayer { get; set; }
    public int RemainingAP { get; set; }

    // Extensible metadata bag for special cases
    public Dictionary<string, object> Meta { get; set; } = new Dictionary<string, object>();

    // Cancellation support usable by coroutines
    public bool CancelRequested { get; set; }
    public void RequestCancel() => CancelRequested = true;

    public GameActionContext() { }  
    public GameActionContext(PlayerController source)
    {
        InitiatingPlayer = source;
    }
    public GameActionContext(Model source)
    {
        SourceModel = source;
    }

    public GameActionContext GetOriginCube ()
    {
        OriginCube = SourceModel?.CurrentCube;
        return this;
    }
    public GameActionContext GetSelectedModel()
    {
        SourceModel = InitiatingPlayer.SelectedModel;
        return this;
    }
    public GameActionContext GetInitiatingPlayer ()
    {
        InitiatingPlayer = SourceModel?.playerControlling;
        return this;
    }
    public GameActionContext SetSelectedPoint (Vector3 point)
    {
        SelectedPoint = point;
        return this;
    }
    public GameActionContext SetTargetModel (Model model)
    {
        TargetModel = model;
        return this;
    }
}
