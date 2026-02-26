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
    [SerializeField] private float pickUpHeightFromCube = 1f; // default height above cube when placed

    public Vector3 SelectedPoint => selectedPoint;

    [Header("Preview Managers")]
    [SerializeField] private float cubeSize = 76.2f; // fallback world units per cube
    [SerializeField] private bool previewMovementRange = false; // toggle in inspector or via UI
    [SerializeField] private MovementPlanner movePlanner;
    // cubes that should be highlighted when movement mode is active
    private List<Cube> highlightedCubes = new List<Cube>();
    private GameObject ghostInstance;

    [Header("Players")]
    [SerializeField] private PlayerBuilder playerBuilder;
    [SerializeField] private List<Player> players;


    private Map currentMap;
    public Map CurrentMap => currentMap;

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

    public void EnableModelMovementMode()
    {
        var mac = CreateModelActionContext();
        movePlanner = new MovementPlanner(mac);
        movingModel = true;
        // compute reachable cubes and cache for visualization
        highlightedCubes.Clear();
        if (mac != null && mac.OriginCube != null)
        {
            var reachable = movePlanner.GetReachableCubes(mac.OriginCube, mac.SourceModel.unit.unitAdvanceSpeed);
            // exclude origin cube from highlights
            foreach (var c in reachable)
            {
                if (c != mac.OriginCube) highlightedCubes.Add(c);
            }
        }
    }
    public void DisableModelMovementMode()
    {
        movePlanner = null;
        movingModel = false;
        highlightedCubes.Clear();
    }

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
                if (selectedModel != null)
                {
                    tabletopCameraController.target = selectedModel.transform;
                    onModelSelected?.Raise(this, selectedModel);
                }
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
            DisableModelMovementMode();
        }

        ShowModelAsGhost();
    }
    private void ShowModelAsGhost()
    {
        // Show a ghost of the model at the selected point, maybe with a transparent material or outline shader, to indicate where it will be placed if the player clicks there.
        // This can help with visualizing the move before committing to it.

        // If there is no selection or no valid target point, remove any existing ghost and return
        if (selectedModel == null || selectedPoint == Vector3.zero)
        {
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            return;
        }

        // Create the ghost if it doesn't exist
        if (ghostInstance == null)
        {
            // Instantiate a clone of the selected model's root GameObject
            ghostInstance = Instantiate(selectedModel.gameObject);
            ghostInstance.name = selectedModel.gameObject.name + "_Ghost";

            // Remove or disable interactive components on the ghost
            var modelComp = ghostInstance.GetComponent<Model>();
            if (modelComp != null) Destroy(modelComp);

            // Disable colliders so it doesn't block raycasts/physics
            foreach (var col in ghostInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Remove rigidbodies from the ghost
            foreach (var rb in ghostInstance.GetComponentsInChildren<Rigidbody>())
            {
                Destroy(rb);
            }

            // Put ghost on Ignore Raycast layer if it exists
            int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreLayer != -1)
            {
                foreach (Transform t in ghostInstance.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = ignoreLayer;
                }
            }

            // Make materials semi-transparent
            foreach (var rend in ghostInstance.GetComponentsInChildren<Renderer>())
            {
                var mats = rend.sharedMaterials;
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    Material baseMat = mats[i] != null ? new Material(mats[i]) : new Material(Shader.Find("Standard"));

                    if (baseMat.HasProperty("_Color"))
                    {
                        Color c = baseMat.color;
                        c.a = 0.5f;
                        baseMat.color = c;
                    }

                    // Try to set standard shader to transparent mode
                    baseMat.SetFloat("_Mode", 3f);
                    baseMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    baseMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    baseMat.SetInt("_ZWrite", 0);
                    baseMat.DisableKeyword("_ALPHATEST_ON");
                    baseMat.EnableKeyword("_ALPHABLEND_ON");
                    baseMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    baseMat.renderQueue = 3000;

                    newMats[i] = baseMat;
                }
                rend.materials = newMats;
            }
        }

        // Position and orient the ghost at the selectedPoint, preserving the vertical offset used when picking up pieces
        Vector3 ghostPos = new Vector3(selectedPoint.x, selectedPoint.y + pickUpHeightFromCube, selectedPoint.z);
        ghostInstance.transform.position = ghostPos;
        ghostInstance.transform.rotation = selectedModel.transform.rotation;
        ghostInstance.transform.localScale = selectedModel.transform.localScale;
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
        if (ghostInstance != null)
            Destroy(ghostInstance.gameObject);

        // move the model
        selectedModel
            .ActionController
            .TryPerformAction(
                new AdvanceAction(), 
                CreateModelActionContext().SetSelectedPoint (selectedPoint));
    }

    /// <summary>
    /// Takes the current state of the board, map, players, and models and pases that into a UnitActionContext to give to any UnitAction object
    /// </summary>
    private ModelActionContext CreateModelActionContext ()
    {
        if (selectedModel == null) return null; // if we don't have a selected model, then we shouldn't event be doing an action.

        var mac = new ModelActionContext(selectedModel)

        // can fill in any other needed context items here
        // using the Builder pattern
            .GetTabletop()
            .GetMap()
            .GetOriginCube()
            .GetInitiatingPlayer();

        // return the final product
        return mac;
    }








    private void OnDrawGizmos()
    {
        // draw highlights for reachable cubes when movement mode is active
        if (movingModel && highlightedCubes != null && highlightedCubes.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var cube in highlightedCubes)
            {
                if (cube == null) continue;
                // draw a thin wire-cube at the bottom face of the cube
                var half = cube.worldSize * 0.5f;
                float bottomY = cube.worldPosition.y - half.y;
                // make the highlight very thin on Y so it appears as a bottom-face outline
                Vector3 center = new Vector3(cube.worldPosition.x, bottomY + 0.01f, cube.worldPosition.z);
                Vector3 size = new Vector3(cube.worldSize.x, 0.02f, cube.worldSize.z);
                Gizmos.DrawWireCube(center, size);
            }
        }

        if (selectedPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(selectedPoint, 1f);
        }
    }
}
