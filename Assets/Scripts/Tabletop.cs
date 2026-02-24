using Assets.Scripts;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// Tabletop now handles input/selection and delegates visualization and planning to dedicated components.
public class Tabletop : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Events")]
    [SerializeField] private GameEvent onGameStart;
    [SerializeField] private GameEvent onModelSelected;
    [SerializeField] private GameEvent onModelDeselected;
    [SerializeField] private GameEvent onModelMoveDeactivated;

    [Header("Point-Click Movement")]
    [SerializeField] private bool movingModel = false;
    [SerializeField] private Model selectedModel;
    [SerializeField] private Vector3 selectedPoint = Vector3.zero;

    [Header("Preview Managers")]
    [SerializeField] private GhostPreviewManager ghostManager;
    [SerializeField] private MovementRangeVisualizer rangeVisualizer;
    [SerializeField] private float cubeSize = 76.2f; // fallback world units per cube
    [SerializeField] private bool previewMovementRange = false; // toggle in inspector or via UI

    [Header("Players")]
    [SerializeField] private PlayerBuilder playerBuilder;
    [SerializeField] private List<Player> players;

    private Map currentMap;
    private MovementPlanner planner;

    void Start()
    {
        if (playerBuilder == null) TryGetComponent<PlayerBuilder>(out playerBuilder);

        onGameStart.Raise(this, null);
    }

    void Update()
    {
        var mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        DoModelPointAndClickMove(mousePos, ray);

        // keep track of activation order
        // for now only one model exists in the world

        // Get Selected models remaining AP and display on UI
        // Show Action buttons for actions that can be taken with the remaining AP
    }

    public void EnableModelMovementMode() => movingModel = true;
    public void DisableeModelMovementMode() => movingModel = false;

    // UI hook
    public void ToggleMovementPreview()
    {
        previewMovementRange = !previewMovementRange;
        if (!previewMovementRange) rangeVisualizer?.ClearMarkers();
        else if (selectedModel != null && planner != null) ShowMovementRangePreview(selectedModel);
    }

    private void DoModelPointAndClickMove(Vector3 mousePos, Ray ray)
    {
        if (selectedModel == null && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Model hitModel = null;
                var go = hitInfo.collider.gameObject;
                if (!go.TryGetComponent(out hitModel))
                {
                    hitModel = go.GetComponentInParent<Model>();
                }

                selectedModel = hitModel;
                if (selectedModel != null) onModelSelected?.Raise(this, selectedModel);

                //if (previewMovementRange && selectedModel != null)
                //{
                //    ShowMovementRangePreview(selectedModel);
                //}
            }
            return;
        }

        if (movingModel && selectedModel != null && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                selectedPoint = hitInfo.point;
            }
        }

        if (selectedModel != null && Input.GetAxis("Cancel") == 1)
        {
            var prev = selectedModel;
            selectedPoint = Vector3.zero;
            selectedModel = null;

            ghostManager?.HideGhost();
            rangeVisualizer?.ClearMarkers();

            onModelDeselected?.Raise(this, prev);
            onModelMoveDeactivated?.Raise(this, prev);
            DisableeModelMovementMode();
        }

        //ShowModelAsGhost();
    }

    private void ShowModelAsGhost()
    {
        if (selectedModel == null || selectedPoint == Vector3.zero)
        {
            ghostManager?.HideGhost();
            rangeVisualizer?.HideRangeCircle();
            return;
        }

        if (planner == null) planner = currentMap != null ? new MovementPlanner(currentMap) : null;

        Vector3 clampedPoint = selectedPoint;
        bool inRange = true;
        if (planner != null && selectedModel.CurrentCube != null && selectedModel.unit != null)
        {
            clampedPoint = planner.ClampPointToRange(selectedModel.CurrentCube, selectedPoint, selectedModel.unit.unitAdvanceSpeed);
            float maxDist = selectedModel.unit.unitAdvanceSpeed * (currentMap != null ? currentMap.CubeSize : cubeSize);
            inRange = Vector3.Distance(selectedModel.CurrentCube.worldPosition, clampedPoint) <= maxDist + 0.01f;
        }

        float yOffset = 0f;
        if (selectedModel.CurrentCube != null)
        {
            yOffset = selectedModel.transform.position.y - selectedModel.CurrentCube.worldPosition.y;
        }
        Vector3 ghostPos = new Vector3(clampedPoint.x, clampedPoint.y + yOffset, clampedPoint.z);

        ghostManager?.ShowGhost(selectedModel.gameObject, ghostPos, selectedModel.transform.rotation, inRange);

        if (selectedModel.CurrentCube != null && selectedModel.unit != null)
        {
            float radius = selectedModel.unit.unitAdvanceSpeed * (currentMap != null ? currentMap.CubeSize : cubeSize);
            rangeVisualizer?.ShowRangeCircle(selectedModel.CurrentCube.worldPosition, radius);
        }
    }
    private void ShowMovementRangePreview(Model model)
    {
        rangeVisualizer?.ClearMarkers();
        if (model == null || model.CurrentCube == null || model.unit == null || currentMap == null) return;
        if (planner == null) planner = new MovementPlanner(currentMap);

        var reachable = planner.GetReachableCubes(model.CurrentCube, model.unit.unitAdvanceSpeed);
        rangeVisualizer?.ShowMarkers(reachable);
    }

    public void OnMapCreated(Component sender, object data)
    {
        if (data is Map map)
        {
            currentMap = map;
            planner = new MovementPlanner(map);
        }
    }

    private void OnDrawGizmos()
    {
        if (selectedPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(selectedPoint, 1f);
        }
    }
}
