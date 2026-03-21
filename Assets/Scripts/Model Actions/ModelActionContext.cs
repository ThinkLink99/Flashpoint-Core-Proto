using System.Collections.Generic;
using UnityEngine;

public class ModelActionContext
{
    public Tabletop Tabletop { get; private set; }
    public Map Map { get; private set; }
    public Vector3 SelectedPoint { get; private set; }
    public Cube OriginCube { get; private set;  }
    public Model SourceModel { get; private set; }
    public Model TargetModel { get; private set; }
    public PlayerController InitiatingPlayer { get; private set; }
    public int RemainingAP { get; private set; }

    // Extensible metadata bag for special cases
    public Dictionary<string, object> Meta { get; private set; } = new Dictionary<string, object>();

    // Cancellation support usable by coroutines
    public bool CancelRequested { get; private set; }
    public void RequestCancel() => CancelRequested = true;

    public ModelActionContext(Model source)
    {
        SourceModel = source;
    }

    public ModelActionContext GetTabletop()
    {
        this.Tabletop = SourceModel?.tabletop;
        return this;
    }
    public ModelActionContext GetTabletop (Tabletop tabletop)
    {
        this.Tabletop = tabletop;
        return this;
    }
    public ModelActionContext GetMap ()
    {
        if (this.Tabletop == null) return this;

        this.Map = Tabletop.currentMap;

        return this;
    }
    public ModelActionContext GetOriginCube ()
    {
        OriginCube = SourceModel?.CurrentCube;
        return this;
    }
    public ModelActionContext GetInitiatingPlayer ()
    {
        InitiatingPlayer = SourceModel?.playerControlling;
        return this;
    }

    public ModelActionContext SetSelectedPoint (Vector3 point)
    {
        SelectedPoint = point;
        return this;
    }
    public ModelActionContext SetTargetModel (Model model)
    {
        TargetModel = model;
        return this;
    }
}