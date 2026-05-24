using System;
using UnityEngine;

public class ModelSelectedEventArgs : EventArgs
{
    public Model Model { get; }
    public PlayerController Requester { get; }

    public ModelSelectedEventArgs(Model model, PlayerController requester)
    {
        Model = model;
        Requester = requester;
    }
}

public class DestinationSelectedEventArgs : EventArgs
{
    public Vector3 Destination { get; }
    public PlayerController Requester { get; }

    public DestinationSelectedEventArgs(Vector3 destination, PlayerController requester)
    {
        Destination = destination;
        Requester = requester;
    }
}

public class TargetSelectedEventArgs : EventArgs
{
    public Model Target { get; }
    public PlayerController Requester { get; }

    public TargetSelectedEventArgs(Model target, PlayerController requester)
    {
        Target = target;
        Requester = requester;
    }
}

public class ModelMovedEventArgs : EventArgs
{
    public Model Model { get; }
    public Vector3 Destination { get; }
    public PlayerController Requester { get; }

    public ModelMovedEventArgs(Model model, Vector3 destination, PlayerController requester)
    {
        Model = model;
        Destination = destination;
        Requester = requester;
    }
}

public class ModelShotEventArgs : EventArgs
{
    public Model Source { get; }
    public Model Target { get; }
    public PlayerController Requester { get; }

    public ModelShotEventArgs(Model source, Model target, PlayerController requester)
    {
        Source = source;
        Target = target;
        Requester = requester;
    }
}
