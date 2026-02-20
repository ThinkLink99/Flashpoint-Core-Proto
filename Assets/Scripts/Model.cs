using UnityEngine;

public class Model : MonoBehaviour
{
    [Header("Events")]
    public GameEvent onModelMoved;

    [Header("Model Details")]
    public Tabletop tabletop;
    public Unit unit; 

    private GameObject basePrefab;
    private GameObject hitBox;

    private Cube currentCube;

    private Vector3 lastPosition = Vector3.zero;

    public Cube CurrentCube { get => currentCube; }

    // Start is called before the first frame update
    void Start()
    {
        basePrefab = this.transform.GetChild(0).gameObject;
        hitBox = this.transform.GetChild(1).gameObject;

        tabletop = FindObjectOfType<Tabletop>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tabletop == null) return;
        if (unit == null) return;

        if (lastPosition != this.transform.localPosition)
        {
            Debug.Log("Model moved to: " + this.transform.localPosition);
            onModelMoved.Raise(this, this.transform.localPosition);
            lastPosition = this.transform.localPosition;
        }
    }

    public void ChangeCube (Cube cube)
    {
        currentCube = cube;
    }

    private void OnDrawGizmos()
    {
        if (unit != null && basePrefab != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, unit.baseSizeMM / 2);
        }

        if (currentCube != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(currentCube.worldPosition, currentCube.worldSize);
        }
    }
}
