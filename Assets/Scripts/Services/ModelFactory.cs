using System;
using UnityEngine;

[Serializable]
public class ModelFactory : IModelFactory
{
    public Model Create (ModelConfiguration configuration)
    {
        GameObject model = UnityEngine.Object.Instantiate (configuration.Model);
        Model modelComponent = model.GetComponent<Model> ();
        modelComponent.Initialize(configuration);

        return modelComponent;
    }
}
public interface IModelFactory
{
    Model Create(ModelConfiguration configuration);
}
