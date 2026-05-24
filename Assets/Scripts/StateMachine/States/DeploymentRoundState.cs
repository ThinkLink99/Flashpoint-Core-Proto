using System.Collections.Generic;
using UnityEngine;

public class DeploymentRoundState : BaseGameState
{
    private List<PlayerController> players;
    private bool[] hasDeplopyed;
    private Map currentMap;

    private GameEvent onAllPlayersDeployed;

    public DeploymentRoundState(GameManager gameManager, GameEvent onAllPlayersDeployed) : base(gameManager)
    {
        this.onAllPlayersDeployed = onAllPlayersDeployed;
    }

    public override void OnEnter()
    {
        players = gameManager.players;
        currentMap = gameManager.currentMap;

        hasDeplopyed = new bool[players.Count];
    }
    public override void Update()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log($"Has Deployed: {hasDeplopyed[i]}");
            if (hasDeplopyed[i]) continue;

            // otherwise we need to let the player deploy.
            // For now, just spawn the models in each square of the deployment zone like before
            // then mark the player deployed

            var playerDeploying = players[i];
            var spawner = playerDeploying.modelSpawner;

            // Initialize player state, such as setting up the fireteam, resetting any turn-specific data, etc.
            //if (showDebugLogs) Debug.Log($"Player {name} has started the game with fireteam of {fireteam.Models.Count} models.");

            // temporarily loop through units and ground level deployment cubes and spawn a model of a unit in each cube
            var zone = currentMap.GetZoneForTeam(playerDeploying.team);
            //if (showDebugLogs) Debug.Log($"Zones for {team.ToString()}: {zone.squares.Count}");
            for (int z = 0; z < zone.squares.Count; z++)
            {
                if (playerDeploying.fireteam.Models[z] != null)
                {
                    var worldPos = new Vector3(zone.squares[z].x, 1, zone.squares[z].y) * currentMap.CubeSize;
                    var spawnedModel = spawner.SpawnForPlayer(playerDeploying.fireteam.Models[z].name, playerDeploying, worldPos);
                    playerDeploying.spawnedModels.Add(spawnedModel);
                }
            }

            hasDeplopyed[i] = true;
            if (i == players.Count - 1) 
                // raise an event to say we are done deploying
                onAllPlayersDeployed.Raise(null, players);
        }
    }
}
