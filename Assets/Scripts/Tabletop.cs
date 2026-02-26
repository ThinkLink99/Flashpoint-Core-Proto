using Assets.Scripts;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

// Tabletop now handles input/selection and delegates visualization and planning to dedicated components.
public class Tabletop : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] private TabletopCamera tabletopCameraController;

    [Header("Events")]
    [SerializeField] private GameEvent onGameStart;
    [SerializeField] private GameEvent onModelSelected;
    [SerializeField] private GameEvent onModelDeselected;
    [SerializeField] private GameEvent onModelMoveDeactivated;

    [Header("Point-Click Movement")]
    [SerializeField] private bool movingModel = false;
    [SerializeField] private Model selectedModel;
    [SerializeField] private Vector3 selectedPoint = Vector3.zero;
    public Vector3 SelectedPoint => selectedPoint;

    [Header("Preview Managers")]
    [SerializeField] private float cubeSize = 76.2f; // fallback world units per cube
    [SerializeField] private bool previewMovementRange = false; // toggle in inspector or via UI

    [Header("Players")]
    [SerializeField] private PlayerBuilder playerBuilder;
    [SerializeField] private List<Player> players;

    private Map currentMap;

    void Start()
    {
        if (playerBuilder == null) TryGetComponent<PlayerBuilder>(out playerBuilder);
        if (tabletopCameraController == null) mainCamera.TryGetComponent<TabletopCamera>(out tabletopCameraController);

        onGameStart.Raise(this, null);
    }

    void Update()
    {
        var mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        DoModelSelect(mousePos, ray);
        DoModelPointAndClickMove(mousePos, ray);

        // keep track of activation order
        // for now only one model exists in the world

        // Get Selected models remaining AP and display on UI
        // Show Action buttons for actions that can be taken with the remaining AP
    }

    private bool PointerOverUI()
    {
        // Prevent world interaction when clicking/pressing UI
        if (EventSystem.current == null) return false;

        // Mouse (editor / standalone)
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        // Touch (mobile): check all touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId)) return true;
        }

        return false;
    }

    public void EnableModelMovementMode() => movingModel = true;
    public void DisableeModelMovementMode() => movingModel = false;

    private void DoModelSelect (Vector3 mousePos, Ray ray)
    {
        if (PointerOverUI()) return;
        if (movingModel) return;

        if (Input.GetMouseButtonDown(0))
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
                tabletopCameraController.target = selectedModel.transform;

                if (selectedModel != null) onModelSelected?.Raise(this, selectedModel);

                //if (previewMovementRange && selectedModel != null)
                //{
                //    ShowMovementRangePreview(selectedModel);
                //}
            }
            return;
        }
    }
    private void DoModelPointAndClickMove(Vector3 mousePos, Ray ray)
    {
        if (PointerOverUI()) return;

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

            onModelDeselected?.Raise(this, prev);
            onModelMoveDeactivated?.Raise(this, prev);
            DisableeModelMovementMode();
        }

        //ShowModelAsGhost();
    }


    public void OnMapCreated(Component sender, object data)
    {
        if (data is Map map)
        {
            currentMap = map;
        }
    }
    public void OnModelMoveConfirmed (Component component, object data)
    {
        movingModel = false;

        // move the model
        selectedModel.ActionController.TryPerformAction(new AdvanceAction());
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
