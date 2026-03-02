using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    public ModelSetScriptableObject spawnableUnits;

    public Material playerTeamMaterial;
    public Material enemyTeamMaterial;

    // Start is called before the first frame update
    public void SpawnModels()
    {
        var model = Instantiate(spawnableUnits.units["Spartan MK VII"]);
        model.name = "Player Model";
        model.transform.localPosition = new Vector3(0, 20, 0);
        model.GetComponentInChildren<MeshRenderer>().material = playerTeamMaterial;
    }

    public GameObject SpawnForPlayer (string unitName, Player player, Vector3 position)
    {
        var model = Instantiate(spawnableUnits.units[unitName]);
        model.name = $"{player.name}'s {unitName}";
        model.transform.localPosition = position;
        model.GetComponentInChildren<MeshRenderer>().material = new Material(playerTeamMaterial);
        return model;
    }
}
