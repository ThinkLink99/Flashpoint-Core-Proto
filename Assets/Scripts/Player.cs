using System.Collections;
using System.Linq;
using UnityEngine;

// Player handles the player's team of models and any player-specific data or actions.
// Such as handling player input, managing the player's turn, and so on. For now, it just holds a reference to a model.
public class Player : MonoBehaviour
{
    public UnitSetScriptableObject spawnableUnits;
    public Material redTeamMaterial;
    public Material blueTeamMaterial;


    // A player can have a team of models, but for now we will just have one model per player.
    [SerializeField] private TeamId team;
    [SerializeField] private Fireteam fireteam;

    [SerializeField] private GameEvent onTurnStarted;
    [SerializeField] private GameEvent onTurnEnded;

    public void OnMapCreated (Component component, object data)
    {
        if (data is Map map)
        {
            // Initialize player state, such as setting up the fireteam, resetting any turn-specific data, etc.
            Debug.Log($"Player {name} has started the game with fireteam of {fireteam.Models.Count} models.");

            // temporarily loop through units and ground level deployment cubes and spawn a model of a unit in each cube
            var zone = map.GetZoneForTeam (team);
            Debug.Log($"Zones for {team.ToString()}: {zone.squares.Count}");
            for (int i = 0; i < zone.squares.Count; i++)
            {
                if (fireteam.Models[i] != null)
                {
                    SpawnModel(fireteam.Models[i], new Vector3Int(zone.squares[i].x, 1, zone.squares[i].y), map.CubeSize);
                }
            }
        }
    }

    public void SpawnModel(Unit unit, Vector3 worldPosition)
    {
        var model = Instantiate(spawnableUnits.units[unit.name]);
        model.name = unit.name;
        model.transform.localPosition = worldPosition;
        switch (team)
        {
            case TeamId.Red:
                model.GetComponentInChildren<MeshRenderer>().material = new Material(redTeamMaterial);
                break;
            case TeamId.Blue:
                model.GetComponentInChildren<MeshRenderer>().material = new Material(blueTeamMaterial);
                break;
        }
    }

    public void SpawnModel(Unit unit, Vector3Int gridPos, float cubeSize)
    {
        var model = Instantiate(spawnableUnits.units[unit.name]);
        model.name = unit.name;
        model.transform.localPosition = new Vector3 (gridPos.x * cubeSize, gridPos.y * cubeSize, gridPos.z * cubeSize);
        switch (team)
        {
            case TeamId.Red:
                model.GetComponentInChildren<MeshRenderer>().material = new Material(redTeamMaterial);
                break;
            case TeamId.Blue:
                model.GetComponentInChildren<MeshRenderer>().material = new Material(blueTeamMaterial);
                break;
        }
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
