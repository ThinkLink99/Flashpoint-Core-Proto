using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class GameDebug : MonoBehaviour
{
    [SerializeField] private GameObject monitorPlayerModel;
    [SerializeField] private GameObject monitorEnemyModel;

    [SerializeField] private TextMeshProUGUI xText;
    [SerializeField] private TextMeshProUGUI yText;
    [SerializeField] private TextMeshProUGUI zText;
    [SerializeField] private TextMeshProUGUI losText;
    [SerializeField] private TextMeshProUGUI visibilityText;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        // If not assigned in the inspector, try to find GameObjects by name in the scene.
        if (monitorPlayerModel == null)
        {
            monitorPlayerModel = GameObject.Find("Player Model");
        }
        if (monitorEnemyModel == null)
        {
            monitorEnemyModel = GameObject.Find("Enemy Model");
        }

        //var monitorPlayerModelComponent = monitorPlayerModel.GetComponent<Model>();
        //var monitorEnemyModelComponent = monitorEnemyModel.GetComponent<Model>();

        //if (monitorPlayerModelComponent.HasLineOfSight(monitorEnemyModelComponent))
        //{
        //    losText.text = "YES";
        //}
        //else
        //{
        //    losText.text = "NO";
        //}

        //if (monitorPlayerModelComponent.HasLineOfSight(monitorEnemyModelComponent))
        //{
        //    visibilityText.text = monitorPlayerModelComponent.PercentageOfModelSeen(monitorEnemyModelComponent).ToString("F2") + "%";
        //}
        //else
        //{
        //    visibilityText.text = "0%";
        //}
    }

    private void OnDestroy()
    {

    }

    private void OnDrawGizmos()
    {
        //// draw sight line
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(eyes.transform.position, eyes.transform.position + eyes.transform.forward * 100);
    }

    public void modelMovedHandler (Component sender, object data)
    {
        Debug.Log("Model moved event received in GameDebug.");

        if (sender is not Model model) return;

        Vector3 modelPosition = (Vector3)data;
        xText.text = $"X: {modelPosition.x.ToString("F2")}";
        yText.text = $"Y: {modelPosition.y.ToString("F2")}";
        zText.text = $"Z: {modelPosition.z.ToString("F2")}";
    }
}
