using Unity.Properties;
using UnityEngine;

public class Model : MonoBehaviour
{
    [Header("Events")]
    public GameEvent onModelMoved;

    [Header("Model Details")]
    public GameManager gameManager;

    [CreateProperty]
    public ModelConfiguration ModelConfiguration;
    [SerializeField] private int currentHealth = 0;

    [Header("Model Controllers")]
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

        // set dynamic values based on configuration, such as health, action points, etc.
        currentHealth = modelConfiguration.unitHP;
    }

    // Start is called before the first frame update
    void Start()
    {
        basePrefab = this.transform.GetChild(0).gameObject;
        hitBox = this.transform.GetChild(1).gameObject;

        gameManager = FindAnyObjectByType<GameManager>();
        actionController = GetComponent<ModelActionController>();
        playerControlling = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager == null) return;
        if (ModelConfiguration == null) return;

        if (lastPosition != this.transform.localPosition)
        {
            onModelMoved.Raise(this, this.transform.localPosition);
            lastPosition = this.transform.localPosition;
        }
    }

    private void Die()
    {
        // for now, just destroy the model. We can add death animations, ragdolls, etc. later.
        Destroy(this.gameObject);

        // Fire off a debug Log so we can see when a model dies. We can replace this with an event later if we want to trigger other things on death, such as checking for end of game conditions, triggering death animations, etc.
        Debug.Log($"{this.gameObject.name} has died.");
    }

    public void ChangeCube (Cube cube)
    {
        currentCube = cube;
    }
    public void MoveModelToPoint (Vector3 point)
    {
        this.transform.localPosition = point;
    }

    public void Wound (int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // for now, just destroy the model. We can add death animations, ragdolls, etc. later.
            Die();
        }
    }

    // Helper: create a lightweight ghost clone (no Model, no physics, on IgnoreRaycast layer)
    public GameObject CreateGhostInstance()
    {
        var ghost = Instantiate(this.gameObject);
        ghost.name = this.gameObject.name + "_Ghost";

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