using System;

// Lightweight view model that exposes read-only state from a ModelActionController
public class ModelActionViewModel
{
    public Model Model { get; }
    public ModelActionController ActionController { get; }

    public bool IsActivated => ActionController?.IsActivated ?? false;
    public bool HasActivated => ActionController?.HasActivated ?? false;
    public bool HasMoved => ActionController?.HasMoved ?? false;
    public bool HasShot => ActionController?.HasShot ?? false;
    public int RemainingAP => ActionController != null ? ActionController.RemainingAP : 0;

    public event Action StateChanged;

    private bool lastIsActivated;
    private bool lastHasActivated;
    private bool lastHasMoved;
    private bool lastHasShot;
    private int lastRemainingAP;

    public ModelActionViewModel(Model model)
    {
        Model = model;
        ActionController = model?.ActionController;

        lastIsActivated = IsActivated;
        lastHasActivated = HasActivated;
        lastHasMoved = HasMoved;
        lastHasShot = HasShot;
        lastRemainingAP = RemainingAP;
    }

    // Call periodically (cheap) to detect state changes and notify listeners
    public void Refresh()
    {
        if (IsActivated != lastIsActivated ||
            HasActivated != lastHasActivated ||
            HasMoved != lastHasMoved ||
            HasShot != lastHasShot ||
            RemainingAP != lastRemainingAP)
        {
            lastIsActivated = IsActivated;
            lastHasActivated = HasActivated;
            lastHasMoved = HasMoved;
            lastHasShot = HasShot;
            lastRemainingAP = RemainingAP;
            StateChanged?.Invoke();
        }
    }

    // Convenience methods for local feedback. Authoritative changes should go through GameManager.
    public void RequestActivate() => ActionController?.BeginActivation();
    public void RequestDeactivate()
    {
        if (ActionController == null) return;
        ActionController.HasActivated = false;
        ActionController.IsActivated = false;
    }
}