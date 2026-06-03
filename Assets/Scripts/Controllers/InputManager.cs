using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController localPlayer; // the PlayerController this client owns
    [SerializeField] private bool allowPreview = true;     // can always preview any model locally
    private bool allowCommands => localPlayer != null && localPlayer.AllowCommands;

    // Right-drag tracking for Civ V style movement
    private bool isRightDragging = false;
    private Vector3 lastPreviewPoint = Vector3.zero;
    [SerializeField] private float pickUpHeightFromCube = 1f; // default height above cube when placed

    void Awake()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (localPlayer == null) localPlayer = FindAnyObjectByType<PlayerController>(); // set properly in game init

        gameManager.OnPlayersCreated += OnAllPlayersCreated; // example of subscribing to GameManager events to know when to allow commands, etc.
    }

    void Update()
    {


        // Left Click (Unit Selection, Target Selection, etc)
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var layerMask = LayerMask.GetMask("Model");
            if (!Physics.Raycast(ray, out RaycastHit hit, 100000000, layerMask)) return;

            var go = hit.collider.gameObject;
            Model hitModel = go.TryGetComponent<Model>(out var m) ? m : go.GetComponentInParent<Model>();
            if (hitModel == null) return;

            // Local-only preview: show info/ghost/etc. (no authoritative calls)
            if (allowPreview)
            {
                var ui = FindAnyObjectByType<ModelActionView>();
                ui?.OnModelSelected(this, hitModel);
            }

            // Command submission: only when client is allowed to command
            if (allowCommands)
            {
                // Send the request to the authoritative GameManager. In networked mode this
                // method can be converted to RPC forwarding to the server.
                if (localPlayer != null && localPlayer.IsTargetingModel)
                {
                    // Tell authoritative manager which target was selected
                    gameManager.RequestSelectTarget(hitModel, localPlayer);

                    // Prefer calling the overload that supplies both source and target so
                    // the GameManager has an explicit SourceModel when executing the action.
                    var source = localPlayer.SelectedModel ?? gameManager.Context.SourceModel;
                    if (source != null)
                        gameManager.RequestShoot(source, hitModel, localPlayer.SelectedWeapon, localPlayer);
                    else
                        // fallback: try the parameterless overload which uses Context.SourceModel
                        gameManager.RequestShoot(localPlayer);
                }
                else
                {
                    gameManager.RequestSelectModel(hitModel, localPlayer);
                }
            }
        }

        // Start Movement Mode if a unit is selected
        if (Input.GetMouseButtonDown(1))
        {
            if (allowCommands && localPlayer != null && 
                localPlayer.SelectedModel != null && 
                localPlayer.SelectedModel.ActionController.HasMoved == false)
            {
                localPlayer.EnableModelMovementMode();

                isRightDragging = true;
                lastPreviewPoint = Vector3.zero;

                // initial destination preview (if pointing at world)
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    lastPreviewPoint = hit.point;
                    gameManager.RequestSelectDestination(lastPreviewPoint, localPlayer);
                }

            }
        }

        // Are we holding right click and in move mode?
        if (isRightDragging && Input.GetMouseButton(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var layerMask = LayerMask.GetMask("Default", "Terrain");
            if (Physics.Raycast(ray, out RaycastHit hit, 100000000, layerMask))
            {
                var point = hit.point;
                // throttle updates by comparing to last preview point to avoid spamming events
                if ((point - lastPreviewPoint).sqrMagnitude > 0.001f)
                {
                    lastPreviewPoint = point;
                    gameManager.RequestSelectDestination(point, localPlayer);
                }
            }
            //Debug.DrawLine(localPlayer.SelectedModel.transform.position, hit.point, Color.green, 1f);
        }

        // If the player releases right click we need to check if they are 
        // 1. moused over a new cube
        // 2. moused over a valid cube
        // If both of these are true, make a request to game manager to move our unit.
        // Otherwise, cancel the move and disable the movement mode in the player UI.
        if (Input.GetMouseButtonUp(1))
        {
            if (allowCommands && localPlayer.IsMovingModel)
            {
                

                // finalize destination (use lastPreviewPoint if we have one, otherwise raycast now)
                var finalPoint = lastPreviewPoint;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var layerMask = LayerMask.GetMask("Default", "Terrain");
                if (finalPoint == Vector3.zero && Physics.Raycast(ray, out RaycastHit hit, 100000000, layerMask)) finalPoint = hit.point;

                // check if the point is in the same cube we started in
                if (localPlayer.MovePlanner.GetCubeContainingPoint(finalPoint) == localPlayer.SelectedModel.CurrentCube)
                {

                }
                else
                {
                    // request authoritative move if possible
                    var source = localPlayer?.SelectedModel ?? gameManager.Context.SourceModel;
                    if (allowCommands && source != null)
                    {
                        gameManager.RequestMove(source, finalPoint, localPlayer);
                    }
                }

                // exit preview mode locally
                localPlayer.DisableModelMovementMode();
                isRightDragging = false;
                lastPreviewPoint = Vector3.zero;
            }
        }
    }

    public void OnAllPlayersCreated (object sender, PlayersCreatedEventArgs e)
    {
        // Example of subscribing to GameManager events to know when to allow commands, etc.
        localPlayer = e.Players[0];
        localPlayer.AllowCommands = true;
    }
}