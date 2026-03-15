using System;
using UnityEngine;

[Serializable]
public class ModelFactory : IModelFactory
{
    [SerializeField] public Material blueTeamMaterial;
    [SerializeField] public Material redTeamMaterial;


    public Model Create (ModelConfiguration configuration)
    {
        GameObject model = UnityEngine.Object.Instantiate (configuration.Model);
        Model modelComponent = model.GetComponent<Model> ();
        modelComponent.Initialize(configuration);

        // Set team color here maybe
        model.GetComponentInChildren<MeshRenderer>().material = new Material(blueTeamMaterial);

        return modelComponent;
    }
}
public interface IModelFactory
{
    Model Create(ModelConfiguration configuration);
}
