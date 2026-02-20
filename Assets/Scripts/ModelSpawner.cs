using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    public GameObject testModel;

    public Material playerTeamMaterial;
    public Material enemyTeamMaterial;

    // Start is called before the first frame update
    public void SpawnModels()
    {
        var model = Instantiate(testModel);
        model.name = "Player Model";
        model.transform.localPosition = new Vector3(0, 20, 0);
        model.GetComponentInChildren<MeshRenderer>().material = playerTeamMaterial;

        //var enemyModel = Instantiate(testModel);
        //enemyModel.name = "Enemy Model";
        //enemyModel.transform.localPosition = new Vector3(300, 20, 300);
        //enemyModel.GetComponentInChildren<MeshRenderer>().material = enemyTeamMaterial;

    }
}
