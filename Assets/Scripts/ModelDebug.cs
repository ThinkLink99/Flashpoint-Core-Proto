using Assets.Scripts;
using TMPro;
using UnityEngine;

public class ModelDebug : MonoBehaviour
{
    [SerializeField] private GameObject monitorModel;

    [SerializeField] private TextMeshPro xText;
    [SerializeField] private TextMeshPro yText;
    [SerializeField] private TextMeshPro zText;
    [SerializeField] private TextMeshPro losText;
    [SerializeField] private TextMeshPro visibilityText;


    private ProcessorDelegate<Vector3, float> distanceToPlayer;
    // Start is called before the first frame update
    void Start()
    {
        // If not assigned in the inspector, try to find GameObjects by name in the scene.
        if (monitorModel == null)
        {
            monitorModel = this.GetComponentInParent<Model>().gameObject;
        }

        distanceToPlayer = Chain<Vector3, float>.Start(new DistanceFromPlayer(monitorModel.transform))
            .Then(new DistanceScorer())
            .Compile();
    }

    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.RotateAround(transform.position, transform.up, 180f);
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
        if (sender is not Model model) return;
        if (model != GetComponentInParent<Model>()) return;

        Vector3 modelPosition = (Vector3)data;
        xText.text = $"X: {modelPosition.x.ToString("F2")}";
        yText.text = $"Y: {modelPosition.y.ToString("F2")}";
        zText.text = $"Z: {modelPosition.z.ToString("F2")}";
    }
}
