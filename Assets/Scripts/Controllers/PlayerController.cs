using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerController : MonoBehaviour
{
    // add near top of class
    public bool IsLocalPlayer { get; set; } = false;    // set by client bootstrap
    public bool AllowCommands { get; set; } = false;    // set when it's this player's turn (or server-authorized)
    public bool AllowPreview { get; set; } = true;      // allow local read-only selection

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
    [SerializeField] private Weapon selectedWeapon = null;
    public Weapon SelectedWeapon => selectedWeapon;

    [SerializeField] private GameInfoSO gameInfo;

    // UI Elements that will need to check against the Models activation status and remaining AP to determine visibility
    [Header("Player UI")]
    [SerializeField] private bool movingModel = false;
    public bool IsMovingModel => movingModel;

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
    public MovementPlanner MovePlanner { get { return movePlanner; } } 

    [Header("Model Targetting")]
    [SerializeField] private bool targettingModel = false;
    [SerializeField] private Model targettedModel;

    // Public read-only view of whether this player is currently selecting a target
    public bool IsTargetingModel => targettingModel;
    public Model TargettedModel => targettedModel;

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
        if (gameManager != null)
        {
            gameManager.OnWorldModelSelected += OnWorldModelSelected;
            gameManager.OnWorldDestinationSelected += OnWorldDestinationSelected;
            gameManager.OnModelShot += OnWorldModelShot;
        }
    }
    public void Start() { }
    public void Update() { }
    public void FixedUpdate()
    {
        if (IsMovingModel) ShowModelAsGhost();

        DrawCubeRange((cube, color) =>
        {
            var rangeIndicator = cube.transform.GetChild(0);

            rangeIndicator.GetComponent<SpriteRenderer>().color = color;
            rangeIndicator.gameObject.SetActive(true);
        });
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnWorldModelSelected -= OnWorldModelSelected;
            gameManager.OnWorldDestinationSelected -= OnWorldDestinationSelected;
            gameManager.OnModelShot -= OnWorldModelShot;
        }
    }

    private void OnWorldModelSelected(object sender, ModelSelectedEventArgs e)
    {
        if (e?.Requester == this)
        {
            selectedModel = e.Model;
        }
    }
    private void OnWorldDestinationSelected(object sender, DestinationSelectedEventArgs e)
    {
        if (e?.Requester == this)
        {
            selectedPoint = e.Destination;
        }
    }
    private void OnWorldModelShot(object sender, ModelShotEventArgs e)
    {
        // If this player was the requester of the shot, exit targeting mode and clear local target
        if (e?.Requester == this)
        {
            targettingModel = false;
            targettedModel = null;
            // also clear any local ghost
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
        }
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
    public void ChangeSelectedWeapon (Weapon weapon)
    {
        selectedWeapon = weapon;
    }

    public void EnableModelMovementMode()
    {
        var mac = gameManager.Context;
        mac.OriginCube = selectedModel.CurrentCube;

        movePlanner = new MovementPlanner(mac);
        movingModel = true;
        // compute reachable cubes and cache for visualization
        advanceHighlightedCubes.Clear();
        sprintHighlightedCubes.Clear();
        if (mac != null && mac.OriginCube != null)
        {
            var advanceReachable = movePlanner.GetReachableCubes(mac.OriginCube, mac.SourceModel.ModelConfiguration.unitAdvanceSpeed);
            var sprintReachable = movePlanner.GetReachableCubes(mac.OriginCube, mac.SourceModel.ModelConfiguration.unitSprintSpeed);

            // exclude origin cube from highlights
            advanceReachable.Remove(mac.OriginCube);
            sprintReachable.Remove(mac.OriginCube);

            advanceHighlightedCubes.AddRange(advanceReachable);
            sprintHighlightedCubes.AddRange(sprintReachable);
        }
    }
    public void DisableModelMovementMode()
    {
        movePlanner = null;
        movingModel = false;

        foreach (var cube in sprintHighlightedCubes)
        {
            if (cube == null) continue;
            var rangeIndicator = cube.transform.GetChild(0);
            rangeIndicator.gameObject.SetActive(false);
        }

        sprintHighlightedCubes.Clear();

        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }
    }
    private void DrawCubeRange (Action<Cube, Color> drawFunc)
    {
        // draw highlights for reachable cubes when movement mode is active
        if (movingModel && sprintHighlightedCubes != null && sprintHighlightedCubes.Count > 0)
        {
            var remaining = sprintHighlightedCubes.Except(advanceHighlightedCubes).ToList();
            foreach (var cube in sprintHighlightedCubes)
            {
                if (cube == null) continue;
                if (!cube.hasTerrainBelow) continue;


                if (cube.PositionIsInCube (selectedPoint))
                {
                    drawFunc(cube, Color.green);
                }
                else if (cube.PositionIsInCube(selectedPoint))
                {
                    drawFunc(cube, Color.red);
                }
                else
                {
                    drawFunc(cube, Color.yellow);
                }
            }
            foreach (var cube in remaining)
            {
                if (cube == null) continue;
                if (!cube.hasTerrainBelow) continue;

                if (cube.PositionIsInCube(selectedPoint))
                {
                    drawFunc(cube, Color.green);
                }
                else
                {
                    drawFunc(cube, Color.orange);
                }
            }
        }
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

    public void OnModelShootingActivated(Component sender, object data)
    {
        targettingModel = true;
    }
    public void OnModelShootingDeactivated(Component sender, object data)
    {
        targettingModel = false;
        targettedModel = null;
    }
    public void OnModelShootingConfirmed(Component sender, object data)
    {
        Debug.Log($"Shoot Confirmed on {targettedModel.name}");

        targettingModel = false;
        targettedModel = null;
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
        DrawCubeRange((cube, color) =>
        {
            Gizmos.color = color;

            // draw a thin wire-cube at the bottom face of the cube
            var half = cube.worldSize * 0.5f;
            float bottomY = cube.worldPosition.y - half.y;
            // make the highlight very thin on Y so it appears as a bottom-face outline
            Vector3 center = new Vector3(cube.worldPosition.x, bottomY + 0.01f, cube.worldPosition.z);
            Vector3 size = new Vector3(cube.worldSize.x, 0.02f, cube.worldSize.z);
            Gizmos.DrawWireCube(center, size);
        });

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