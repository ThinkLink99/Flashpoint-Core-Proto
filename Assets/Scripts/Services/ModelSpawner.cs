using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    [SerializeField] private IModelFactory factory;
    public List<Material> teamColorMaterials;
    public ModelSetScriptableObject configurations;

    private void Awake()
    {
        factory = new ModelFactory();
    }

    public Model SpawnForPlayer (string unitName, PlayerController player, Vector3 worldPosition)
    {
        var model = factory.Create(configurations.units[unitName]);
        model.name = $"{player.name}'s {unitName}";
        model.transform.localPosition = worldPosition;
        model.playerControlling = player;

        model.GetComponentInChildren<MeshRenderer>().material = new Material(teamColorMaterials[(int)player.team]);
        return model;
    }
}
