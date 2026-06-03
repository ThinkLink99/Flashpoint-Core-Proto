using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider))]
public class Cube : MonoBehaviour
{
    public Vector3 worldPosition; // center position in world space
    public Vector3 worldSize; // size in world units (e.g. 1 unit = 1 millimeter)
    public Vector3 mapPosition; // position on the map grid, e.g. (0,0,0) for the first cube, (1,0,0) for the cube to the right of it, etc.

    public BoxCollider boxCollider;

    public TeamId deploymentZoneTeam;
    public bool IsDeploymentZone = false;

    public bool isPassable = true;
    public bool hasTerrainBelow = false;

    public bool HasDetectedEnter = false;
    public bool HasDetectedExit = false;

    [Header("Ground Check")]
    [Tooltip("Fraction of sample rays that must hit terrain for the cube to be considered grounded (0..1).")]
    [Range(0f, 1f)]
    public float requiredGroundCoverage = 0.5f;
    [Range(0f, 1f)]
    [Tooltip("Fraction of sample rays that must hit terrain for the cube to be considered unpassable (0..1).")]
    public float requiredSpaceCoverage = 0.5f;
    [Tooltip("Number of samples per axis (sampleGrid x sampleGrid total rays).")]
    public int sampleGrid = 3;
    [Tooltip("When enabled, draws sampling points and hit/miss gizmos in the scene view.")]
    public bool showSamplingDebug = false;

    private void Awake()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // Ensure cube collider is a trigger so OnTriggerEnter/Exit runs.
            boxCollider.isTrigger = true;
        }
    }
    private void OnValidate()
    {
        // keep inspector-friendly: ensure reference and set trigger in editor
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null) boxCollider.isTrigger = true;
    }
    public void OnEnable()
    {
        //// Lets get our initial values here
        //HasSufficientGround();
    }

    public bool ColliderIntersectsCube(Collider collider)
    {
        return collider.bounds.Intersects(boxCollider.bounds);
    }
    public bool PositionIsInCube(Vector3 position)
    {
        // X_min <= X <= X_max and Y_min <= Y <= Y_max and Z_min <= Z <= Z_max
        float x = position.x;
        float y = position.y;
        float z = position.z;

        float xMin = worldPosition.x - worldSize.x / 2f;
        float xMax = worldPosition.x + worldSize.x / 2f;
        float yMin = worldPosition.y - worldSize.y / 2f;
        float yMax = worldPosition.y + worldSize.y / 2f; // fixed - use /2
        float zMin = worldPosition.z - worldSize.z / 2f;
        float zMax = worldPosition.z + worldSize.z / 2f;

        bool inX = (x <= xMax && x >= xMin);
        bool inY = (y <= yMax && y >= yMin);
        bool inZ = (z <= zMax && z >= zMin);

        return inX && inY && inZ;
    }

    /// <summary>
    /// Perform a quick sampling check to determine if there is "sufficient" terrain directly under this
    /// cube. This casts a grid of short downward rays from the cube top and returns true when the
    /// fraction of rays that hit a Terrain component meets or exceeds requiredCoverage.
    /// </summary>
    public bool HasSufficientGround(float requiredCoverage = 0.5f, int sampleGrid = 3)
    {
        if (sampleGrid <= 0) sampleGrid = 1;
        requiredCoverage = Mathf.Clamp01(requiredCoverage);

        int hits = 0;
        int total = sampleGrid * sampleGrid;

        // top of the cube in world space
        float topY = worldPosition.y + worldSize.y / 2f;
        // length to cast down through the cube volume
        float rayLength = worldSize.y + 0.5f;

        for (int ix = 0; ix < sampleGrid; ix++)
        {
            for (int iz = 0; iz < sampleGrid; iz++)
            {
                // normalized offsets across [-0.5, 0.5] for X and Z
                float u = ((ix + 0.5f) / sampleGrid) - 0.5f;
                float v = ((iz + 0.5f) / sampleGrid) - 0.5f;

                Vector3 origin = new Vector3(
                    worldPosition.x + u * worldSize.x,
                    topY + 0.01f,
                    worldPosition.z + v * worldSize.z);

                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength))
                {
                    if (hit.collider != null && hit.collider.TryGetComponent<Terrain>(out Terrain _))
                    {
                        hits++;
                    }
                    if (showSamplingDebug)
                    {
                        Debug.DrawLine(origin, hit.point, Color.green, 0.1f);
                    }
                }
                else
                {
                    if (showSamplingDebug)
                    {
                        //Debug.DrawRay(origin, Vector3.down * rayLength, Color.red, 0.1f);
                        Debug.DrawLine(origin, origin + Vector3.down * rayLength, Color.red, 0.1f);
                    }
                }
            }
        }


        float coverage = (float)hits / (float)total;
        return coverage >= requiredCoverage;
    }

    /// <summary>
    /// Perform a quick sampling check to determine if there is "sufficient" open space inside of this cube. 
    /// This casts a grid of short upward rays from the cube bottom and returns true when the
    /// fraction of rays that do not hit any obstacles meets or exceeds requiredCoverage.
    /// </summary>
    /// <param name="requiredCoverage"></param>
    /// <param name="sampleGrid"></param>
    /// <returns></returns>
    public bool HasSufficientSpace (float requiredCoverage = 0.5f, int sampleGrid = 3)
    {
        if (sampleGrid <= 0) sampleGrid = 1;
        requiredCoverage = Mathf.Clamp01(requiredCoverage);

        int hits = 0;
        int total = sampleGrid * sampleGrid;

        // bottom of the cube in world space
        float bottomY = worldPosition.y - worldSize.y / 2f;
        // length to cast up through the cube volume
        float rayLength = worldSize.y - 0.5f;

        for (int ix = 0; ix < sampleGrid; ix++)
        {
            for (int iz = 0; iz < sampleGrid; iz++)
            {
                // normalized offsets across [-0.5, 0.5] for X and Z
                float u = ((ix + 0.5f) / sampleGrid) - 0.5f;
                float v = ((iz + 0.5f) / sampleGrid) - 0.5f;

                Vector3 origin = new Vector3(
                    worldPosition.x + u * worldSize.x,
                    bottomY - 0.01f,
                    worldPosition.z + v * worldSize.z);

                if (Physics.Raycast(origin, Vector3.up, out RaycastHit hit, rayLength))
                {
                    if (hit.collider != null && hit.collider.TryGetComponent<Terrain>(out Terrain _))
                    {
                        hits++;
                    }
                    if (showSamplingDebug)
                    {
                        Debug.DrawLine(origin, hit.point, Color.green, 0.1f);
                    }
                }
                else
                {
                    if (showSamplingDebug)
                    {
                        Debug.DrawLine(origin, origin + Vector3.up * rayLength, Color.red, 0.1f);
                    }
                }
            }
        }


        float coverage = (float)hits / (float)total;
        return coverage <= requiredCoverage;
    }

    /// <summary>
    /// Convenience overload that uses the inspector-configured values.
    /// </summary>
    public bool HasSufficientGround()
    {
        return HasSufficientGround(requiredGroundCoverage, sampleGrid);
    }
    public bool HasSufficientSpace()
    {
        return HasSufficientSpace(requiredSpaceCoverage, sampleGrid);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogAssertion ($"Cube at {mapPosition} detected trigger enter with {other.gameObject.name}");
        HasDetectedEnter = true;
        HasDetectedExit = false;


        if (other.gameObject.TryGetComponent<Model>(out Model model))
        {
            model.ChangeCube(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Debug.LogAssertion ($"Cube at {mapPosition} detected trigger exit with {other.gameObject.name}");
        HasDetectedEnter = false;
        HasDetectedExit = true;

        if (other.gameObject.TryGetComponent<Model>(out Model model))
        {
            // only clear if this cube is currently set on the model
            if (model.CurrentCube == this)
            {
                model.ChangeCube(null);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //if (IsDeploymentZone)
        //{
        //    switch (deploymentZoneTeam)
        //    {
        //        case TeamId.Red:
        //            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        //            break;
        //        case TeamId.Blue:
        //            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
        //            break;
        //        default:
        //            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        //            break;
        //    }
        //    Gizmos.DrawWireCube(worldPosition, worldSize);
        //}

        if (isPassable == false)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(worldPosition, worldSize);
        }
        //if (hasTerrainBelow)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireCube(worldPosition, worldSize);
        //}
    }
}
