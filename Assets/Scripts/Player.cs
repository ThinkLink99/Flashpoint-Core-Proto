using System.Collections;
using System.Linq;
using UnityEngine;

// Player handles the player's team of models and any player-specific data or actions.
// Such as handling player input, managing the player's turn, and so on. For now, it just holds a reference to a model.
public class Player : MonoBehaviour
{
    // A player can have a team of models, but for now we will just have one model per player.
    [SerializeField] private Fireteam fireteam;

    [SerializeField] private GameEvent onTurnStarted;
    [SerializeField] private GameEvent onTurnEnded;

    public void OnMapCreated (Component component, object data)
    {
        // Initialize player state, such as setting up the fireteam, resetting any turn-specific data, etc.

        Debug.Log ($"Player {name} has started the game with fireteam of {fireteam.Models.Count} models.");
    }

    void Start()
    {
        
    }
    
    public void BeginTurn()
    {
        // Handle turn start logic, such as resetting AP, enabling input, etc.
        // Eventually this will also need to handle turn order and multiple models per player.
    }
    public void EndTurn()
    {
        // Handle turn end logic, such as disabling input, notifying turn manager, etc.
    }
}
