using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

public class Model : MonoBehaviour, IHasKeywords
{
    [Header("Events")]
    public GameEvent onModelMoved;

    [Header("Model Details")]
    public GameManager gameManager;

    [CreateProperty]
    public ModelConfiguration ModelConfiguration;
    [SerializeField] private int currentHealth = 0;
    public KeywordCollection Keywords { get; } = new KeywordCollection();

    [Header("Model Controllers")]
    [SerializeField] private ModelActionController actionController;
    [SerializeField] public PlayerController playerControlling;

    private GameObject basePrefab;
    private GameObject hitBox;

    private Cube currentCube;

    private Vector3 lastPosition = Vector3.zero;

    public Cube CurrentCube { get => currentCube; }
    public ModelActionController ActionController { get => actionController; }
    public int CurrentHealth { get => currentHealth; }

    public void Initialize (ModelConfiguration modelConfiguration)
    {
        ModelConfiguration = modelConfiguration;

        Keywords.Initialize(modelConfiguration.keywords);

        // set dynamic values based on configuration, such as health, action points, etc.
        currentHealth = modelConfiguration.unitHP;

        // create weapons based on configuration
        foreach (var weaponConfig in modelConfiguration.unitWeapons)
        {
            var weaponObj = new GameObject(weaponConfig.name);
            weaponObj.transform.SetParent(this.transform);
            var weapon = weaponObj.AddComponent<Weapon>();
            weapon.Initialize(weaponConfig, this);
        }
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
    }

    public bool HasKeyword(string keywordId) => Keywords.HasKeyword(keywordId);
    public int GetKeywordValue(string keywordId) => Keywords.GetKeyword(keywordId).Value;
    public RuntimeKeyword GetKeyword (string keywordId) => Keywords.GetKeyword(keywordId);

    public void ChangeCube (Cube cube)
    {
        // Only set the cube if it is passable and has enough ground coverage
        if (cube == null)
        {
            currentCube = null;
            return;
        }

        // respect explicit passability flag first
        if (!cube.isPassable)
        {
            Debug.LogWarning($"Cube at {cube.worldPosition} is marked not passable.");
            return;
        }

        // require sufficient terrain under the cube before allowing a model to occupy it
        //if (!cube.HasSufficientGround())
        //{
        //    Debug.LogWarning($"Cube at {cube.worldPosition} does not have sufficient ground below.");
        //    return;
        //}

        currentCube = cube;
    }
    public void MoveModelToPoint (Vector3 point)
    {
        this.transform.localPosition = point;
    }

    public void Wound (int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} wounded for {damage} damage.");

        if (currentHealth <= 0)
        {
            // for now, just destroy the model. We can add death animations, ragdolls, etc. later.
            Die();
        }
    }
    private void Die()
    {
        // for now, just destroy the model. We can add death animations, ragdolls, etc. later.
        Destroy(this.gameObject);

        // Fire off a debug Log so we can see when a model dies. We can replace this with an event later if we want to trigger other things on death, such as checking for end of game conditions, triggering death animations, etc.
        Debug.Log($"{this.gameObject.name} has died.");
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