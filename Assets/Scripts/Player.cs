using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Player handles the player's team of models and any player-specific data or actions.
// Such as handling player input, managing the player's turn, and so on. For now, it just holds a reference to a model.
public class Player : MonoBehaviour
{
    // A player can have a team of models, but for now we will just have one model per player.
    [SerializeField] private Model[] models => GetComponentsInChildren<Model>();

    [SerializeField] private GameEvent onTurnStarted;
    [SerializeField] private GameEvent onTurnEnded;

    [Header("Test Spawn (temporary)")]
    [SerializeField] private bool spawnTestUnit = true;
    [SerializeField] private string testUnitName = "Spartan MK VII";
    [SerializeField] private Vector3 testSpawnPosition = new Vector3(0, 20, 0);
    [SerializeField] private ModelSpawner modelSpawner;

    void Start()
    {
        if (models == null) Debug.LogError("Player has no model assigned!");

        // Optional: spawn a temporary test unit for this player (useful during dev)
        if (spawnTestUnit)
        {
            if (modelSpawner == null)
            {
                Debug.LogError("ModelSpawner not found in scene. Cannot spawn test unit.");
                return;
            }

            var spawned = modelSpawner.SpawnForPlayer(testUnitName, this, testSpawnPosition);
            if (spawned == null)
            {
                Debug.LogError("SpawnForPlayer returned null.");
                return;
            }

            // Parent the spawned model under this player so GetComponentsInChildren<Model>() finds it
            spawned.transform.SetParent(this.transform, false);

            // Ensure the spawned Model has a Unit assigned. Create a quick temporary Unit if needed.
            var modelComp = spawned.GetComponent<Model>();
            if (modelComp != null && modelComp.unit == null)
            {
                var tempUnit = ScriptableObject.CreateInstance<Unit>();
                tempUnit.unitName = testUnitName;
                modelComp.unit = tempUnit;
            }
        }
    }
    
    public Model GetModel() => models[0];

    public void BeginTurn()
    {
        // Handle turn start logic, such as resetting AP, enabling input, etc.
        // Eventually this will also need to handle turn order and multiple models per player.
        GetModel().ActionController.BeginActivation();
    }
    public void EndTurn()
    {
        // Handle turn end logic, such as disabling input, notifying turn manager, etc.
        GetModel().ActionController.EndActivation();
    }
}
