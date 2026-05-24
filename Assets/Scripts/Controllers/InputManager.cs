using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController localPlayer; // the PlayerController this client owns
    [SerializeField] private bool allowPreview = true;     // can always preview any model locally
    private bool allowCommands => localPlayer != null && localPlayer.AllowCommands;

    void Awake()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (localPlayer == null) localPlayer = FindAnyObjectByType<PlayerController>(); // set properly in game init
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

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
                gameManager.RequestSelectModel(hitModel, localPlayer);
            }
        }
    }

    public void OnAllPlayersDeployed (Component sender , object data )
    {
        // Example of subscribing to GameManager events to know when to allow commands, etc.
        if (data is List<PlayerController> players)
        {
            localPlayer = players[0];
            localPlayer.AllowCommands = true;
        }
    }
}