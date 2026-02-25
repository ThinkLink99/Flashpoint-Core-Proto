using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Cube : MonoBehaviour
{
    public Vector3 worldPosition; // center position in world space
    public Vector3 worldSize; // size in world units (e.g. 1 unit = 1 millimeter)
    public Vector3 mapPosition; // position on the map grid, e.g. (0,0,0) for the first cube, (1,0,0) for the cube to the right of it, etc.

    public BoxCollider boxCollider;

    public TeamId deploymentZoneTeam;
    public bool IsDeploymentZone = false;

    public void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        // Use trigger so that OnTriggerEnter/Exit fire reliably when the Model (with Rigidbody) moves through
        boxCollider.isTrigger = true;
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

        Debug.Log("inX: " + inX + " inY: " + inY + " inZ: " + inZ);

        return inX && inY && inZ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Model>(out Model model))
        {
            model.ChangeCube(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
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
        //Gizmos.color = Color.green;
        if (IsDeploymentZone)
        {
            switch (deploymentZoneTeam)
            {
                case TeamId.Red:
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                    break;
                case TeamId.Blue:
                    Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                    break;
                default:
                    Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                    break;
            }
            Gizmos.DrawWireCube(worldPosition, worldSize);
        }
    }
}
