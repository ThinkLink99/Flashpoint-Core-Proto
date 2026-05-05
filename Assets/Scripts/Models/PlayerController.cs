using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Fireteam Details")]
    [SerializeField] public TeamId team;
    [SerializeField] public Fireteam fireteam;
    [SerializeField] public List<Model> spawnedModels;
    [SerializeField] public ModelSpawner modelSpawner;

    [Header("Turn Information")]
    [SerializeField] public bool isHumanControlled = false;
    [SerializeField] private bool firstTurnInRound = true;
    [SerializeField] private Model selectedModel = null;
    public Model SelectedModel => selectedModel;
    [SerializeField] private List<Model> activationsRemaining;
    [SerializeField] private Model activatedModel = null;
    [SerializeField] private ModelActionController activatedModelActionController = null;

    [SerializeField] private GameInfoSO gameInfo;

    // UI Elements that will need to check against the Models activation status and remaining AP to determine visibility
    [Header("Player UI")]
    [SerializeField] private bool movingModel = false;

    [Header("Model Moving")]
    [SerializeField] private float pickUpHeightFromCube = 1f; // default height above cube when placed
    [SerializeField] private Vector3 selectedPoint = Vector3.zero;
    public Vector3 SelectedPoint => selectedPoint;
    [SerializeField] private float cubeSize = 76.2f; // fallback world units per cube
    [SerializeField] private bool previewMovementRange = false; // toggle in inspector or via UI
    [SerializeField] private MovementPlanner movePlanner;
    private List<Cube> advanceHighlightedCubes = new List<Cube>(); // cubes that should be highlighted when movement mode is active
    private List<Cube> sprintHighlightedCubes = new List<Cube>();
    private GameObject ghostInstance;

    [Header("Model Targetting")]
    [SerializeField] private bool targettingModel = false;
    [SerializeField] private Model targettedModel;

    [Header("Game Events")]
    [SerializeField] private GameEvent onModelSelected;
    [SerializeField] private GameEvent onModelDeselected;
    [SerializeField] private GameEvent onModelMoveDeactivated;
    [SerializeField] private GameEvent onPlayerTurnEnded;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = true;

    [Header("Player States")]
    StateMachine stateMachine;
    [SerializeField] private bool isPlayerTurn = false;

    public List<Model> ActivationsRemaining => activationsRemaining;

    public void Awake()
    {
        activationsRemaining = new List<Model>();
        spawnedModels = new List<Model>();
        gameManager = FindAnyObjectByType <GameManager>();
    }
    public void Start()
    {
        
    }
    public void Update()
    {
        var mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        DoModelSelect(mousePos, ray);
        DoModelPointAndClickMove(mousePos, ray);
    }
    public void FixedUpdate()
    {
    }

    public void BeginTurn ()
    {
        // Handle turn start logic, such as resetting AP, enabling input, etc.
        if (firstTurnInRound)
        {
            // reset activations
            ResetActivations();
        }

        isPlayerTurn = true;
        gameManager.StartTurn(this);
    }
    private void ResetActivations ()
    {
        activationsRemaining.Clear();
        foreach (var model in spawnedModels)
            activationsRemaining.Add(model);
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
    private void DoModelSelect(Vector3 mousePos, Ray ray)
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
                if (hitModel != null)
                {
                    if (targettingModel)
                    {
                        targettedModel = hitModel;
                        gameManager.SelectTarget(targettedModel);
                    }
                    else 
                    {
                        selectedModel = hitModel;
                        //tabletopCameraController.target = selectedModel.transform;
                        gameManager.SelectModel(selectedModel);
                        onModelSelected?.Raise(this, selectedModel);
                    }
                }
            }
            return;
        }
    }

    private void DoModelPointAndClickMove(Vector3 mousePos, Ray ray)
    {
        if (selectedModel != null && Input.GetAxis("Cancel") == 1)
        {
            var prev = selectedModel;
            selectedPoint = Vector3.zero;
            selectedModel = null;

            onModelDeselected?.Raise(this, prev);
            onModelMoveDeactivated?.Raise(this, prev);
            DisableModelMovementMode();
        }

        if (PointerOverUI()) return;

        if (movingModel && selectedModel != null && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                // check if this point falls withing our movement range
                var potentialPoint = hitInfo.point;
                if (potentialPoint == null) return; // shouldnt even happen but we'll check it JIC

                var cubeIn = movePlanner.GetCubeContainingPoint(potentialPoint);
                if (cubeIn == null) return; // there is no cube in the world for this point, don't update the selected point

                if (advanceHighlightedCubes.Contains(cubeIn))
                    // if our cube is in this list, we can update our selected point
                    selectedPoint = hitInfo.point;
            }
        }

        ShowModelAsGhost();
    }
    public void EnableModelMovementMode()
    {
        //var mac = CreateModelActionContext();
        //movePlanner = new MovementPlanner(mac);
        //movingModel = true;
        //// compute reachable cubes and cache for visualization
        //advanceHighlightedCubes.Clear();
        //sprintHighlightedCubes.Clear();
        //if (mac != null && mac.OriginCube != null)
        //{
        //    var advanceReachable = movePlanner.GetReachableCubes(mac.OriginCube, mac.SourceModel.ModelConfiguration.unitAdvanceSpeed);
        //    var sprintReachable = movePlanner.GetReachableCubes(mac.OriginCube, mac.SourceModel.ModelConfiguration.unitSprintSpeed);

        //    // exclude origin cube from highlights
        //    advanceReachable.Remove(mac.OriginCube);
        //    sprintReachable.Remove(mac.OriginCube);

        //    advanceHighlightedCubes.AddRange(advanceReachable);
        //    sprintHighlightedCubes.AddRange(sprintReachable);
        //}
    }
    public void DisableModelMovementMode()
    {
        movePlanner = null;
        movingModel = false;
        sprintHighlightedCubes.Clear();
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
    private bool SelectedModelHasLineOfSight(Model target)
    {
        // draw a ray from eyes of current model to the whole target model
        var ray = new Ray(selectedModel.transform.position, target.transform.position - selectedModel.transform.position);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            return true;
        }

        return false;
    }

    public void EndTurn ()
    {
        // Handle turn end logic, such as disabling input, notifying turn manager, etc.
        isPlayerTurn = false;
        onPlayerTurnEnded?.Raise(this, null);
    }

    public void OnModelDeselected(Component sender, object data)
    {
        if (data is not Model model) return;
        if (selectedModel != model) return;

        selectedModel = null;
    }
    public void OnModelActivated(Component sender, object data)
    {
        activatedModel = selectedModel;
        activatedModelActionController = selectedModel.ActionController;

        activatedModelActionController.BeginActivation();
    }
    public void OnModelActivationCancelled (Component sender, object data)
    {
        activatedModelActionController.HasActivated = false;
        activatedModelActionController.IsActivated = false;

        activatedModel = null;
        activatedModelActionController = null;
    }
    public void OnModelMoveActivated(Component sender, object data)
    {
        movingModel = true;
    }
    public void OnModelMoveDeactivated(Component sender, object data)
    {
        movingModel = false;
    }
    public void OnModelMoveConfirmed(Component component, object data)
    {
        movingModel = false;
        if (ghostInstance != null)
            Destroy(ghostInstance.gameObject);

        // move the model
        gameManager.SelectDestination(selectedPoint);
        gameManager.TryExecuteAction(
                new AdvanceAction());
    }
    public void OnModelShootingStarted(Component sender, object data)
    {
        targettingModel = true;
    }
    public void OnModelShootingFinished(Component sender, object data)
    {
        targettingModel = true;
    }
    public void OnMapCreated(Component component, object data)
    {
        if (data is Map map)
        {
            // Initialize player state, such as setting up the fireteam, resetting any turn-specific data, etc.
            if (showDebugLogs) Debug.Log($"Player {name} has started the game with fireteam of {fireteam.Models.Count} models.");

            // temporarily loop through units and ground level deployment cubes and spawn a model of a unit in each cube
            var zone = map.GetZoneForTeam(team);
            if (showDebugLogs) Debug.Log($"Zones for {team.ToString()}: {zone.squares.Count}");
            for (int i = 0; i < zone.squares.Count; i++)
            {
                if (fireteam.Models[i] != null)
                {
                    var worldPos = new Vector3(zone.squares[i].x, 1, zone.squares[i].y) * map.CubeSize;
                    var model = modelSpawner.SpawnForPlayer(fireteam.Models[i].name, this, worldPos);
                    spawnedModels.Add (model);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // draw highlights for reachable cubes when movement mode is active
        if (movingModel && sprintHighlightedCubes != null && sprintHighlightedCubes.Count > 0)
        {
            Gizmos.color = Color.yellow;
            var remaining = sprintHighlightedCubes.Except(advanceHighlightedCubes).ToList();  
            foreach (var cube in sprintHighlightedCubes)
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
            Gizmos.color = Color.red;
            foreach (var cube in remaining)
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

        if (selectedModel != null && targettedModel != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(selectedModel.transform.position, targettedModel.transform.position);
        }
    }
}