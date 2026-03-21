using UnityEngine;

public class Model : MonoBehaviour
{
    [Header("Events")]
    public GameEvent onModelMoved;

    [Header("Model Details")]
    public Tabletop tabletop;
    public ModelConfiguration ModelConfiguration { get; private set; }
    [SerializeField] private ModelActionController actionController;
    [SerializeField] public PlayerController playerControlling;

    private GameObject basePrefab;
    private GameObject hitBox;

    private Cube currentCube;

    private Vector3 lastPosition = Vector3.zero;

    public Cube CurrentCube { get => currentCube; }
    public ModelActionController ActionController { get => actionController; }

    public void Initialize (ModelConfiguration modelConfiguration)
    {
        ModelConfiguration = modelConfiguration; 
    }

    // Start is called before the first frame update
    void Start()
    {
        basePrefab = this.transform.GetChild(0).gameObject;
        hitBox = this.transform.GetChild(1).gameObject;

        tabletop = FindObjectOfType<Tabletop>();
        actionController = GetComponent<ModelActionController>();
        playerControlling = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tabletop == null) return;
        if (ModelConfiguration == null) return;

        if (lastPosition != this.transform.localPosition)
        {
            onModelMoved.Raise(this, this.transform.localPosition);
            lastPosition = this.transform.localPosition;
        }
    }

    public void ChangeCube (Cube cube)
    {
        currentCube = cube;
    }
    public void MoveModelToPoint (Vector3 point)
    {
        this.transform.localPosition = point;
    }

    // Helper: create a lightweight ghost clone (no Model, no physics, on IgnoreRaycast layer)
    public GameObject CreateGhostInstance()
    {
        var ghost = Instantiate(this.gameObject);
        ghost.name = this.gameObject.name + "_Ghost";

        // Remove gameplay components
        //var modelComp = ghost.GetComponent<Model>();
        //if (modelComp != null) Destroy(modelComp);

        //foreach (var col in ghost.GetComponentsInChildren<Collider>())
        //{
        //    col.enabled = false;
        //}

        //foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>())
        //{
        //    Destroy(rb);
        //}

        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer != -1)
        {
            foreach (Transform t in ghost.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = ignoreLayer;
            }
        }

        return ghost;
    }

    private void OnDrawGizmos()
    {
        if (ModelConfiguration != null && basePrefab != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, ModelConfiguration.baseSizeMM / 2);
        }

        if (currentCube != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(currentCube.worldPosition, currentCube.worldSize);
        }
    }
}
