using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    [SerializeField] private IModelFactory factory;
    public List<ModelConfiguration> configurations = new List<ModelConfiguration>();

    private void Awake()
    {
        factory = new ModelFactory();
    }

    public Model SpawnForPlayer (string unitName, Player player, Vector3 position)
    {
        var model = factory.Create(configurations[0]);
        model.name = $"{player.name}'s {unitName}";
        model.transform.localPosition = position;

        return model;
    }
}
