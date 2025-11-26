using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tabletop : MonoBehaviour
{
    const int BOARD_SIZE_X = 2;
    const int BOARD_SIZE_Y = 2;
    const int BOARD_SIZE_Z = 2;

    public GameObject groundPrefab;
    [SerializeField] public object[,,] grid = new object[BOARD_SIZE_X, BOARD_SIZE_Y, BOARD_SIZE_Z];

    public Camera mainCamera;

    public GameObject testModel;


    // Start is called before the first frame update
    void Start()
    {
        var start = this.transform.localPosition;
        for (int x = 0; x < BOARD_SIZE_X; x++)
        {
            for (int z = 0; z < BOARD_SIZE_Z; z++)
            {
                var ground = Instantiate(groundPrefab);
                ground.transform.localPosition = new Vector3((x * ground.transform.lossyScale.x), (ground.transform.lossyScale.y / 2), (z * ground.transform.lossyScale.z));
                grid[x, 0, z] = ground.AddComponent<Cube>();
                (grid[x, 0, z] as Cube).worldPosition = ground.transform.localPosition;
                (grid[x, 0, z] as Cube).worldSize = ground.transform.lossyScale;
            }
        }

        var model = Instantiate(testModel);
        model.transform.localPosition = new Vector3(0, 1, 0);

        //var enemyModel = Instantiate(testModel);
        //enemyModel.transform.localPosition = (grid[BOARD_SIZE_X - 1, 0, BOARD_SIZE_Z - 1] as Cube).worldPosition;

        mainCamera.transform.LookAt(model.transform);
    }

    // Update is called once per frame
    void Update()
    {
        var mousePos = Input.mousePosition;
        var worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.transform.position.y));

        if (Input.GetMouseButtonDown(0))
        {
            var ray = mainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Debug.Log("Hit: " + hitInfo.collider.gameObject.name);
            }
        }
    }


    public Cube GetCube(Vector3 position)
    {
        for (int x = 0; x < BOARD_SIZE_X; x++)
        {
            for (int y = 0; y < BOARD_SIZE_Y; y++)
            {
                for (int z = 0; z < BOARD_SIZE_Z; z++)
                {
                    var cube = grid[x, y, z] as Cube;
                    if (cube is null) continue;

                    if (cube.PositionIsInCube(position))
                    {
                        return cube;
                    }
                }
            }
        }

        // We found nothing
        Debug.LogError ("GetCube: No cube found at position " + position.ToString());
        return null;
    }
}
public class Cube : MonoBehaviour
{
    public Vector3 worldPosition;
    public Vector3 worldSize;

    public bool PositionIsInCube (Vector3 position)
    {
        // X_min <= X <= X_max and Y_min <= Y <= Y_max  and Z_min <= Z <= Z_max
        float x = position.x;
        float y = position.y;
        float z = position.z;

        float xMin = worldPosition.x;
        float xMax = worldPosition.x + worldSize.x;
        float yMin = worldPosition.y;
        float yMax = worldPosition.y + worldSize.y;
        float zMin = worldPosition.z;
        float zMax = worldPosition.z + worldSize.z;

        bool inX = (x <= xMax && x >= xMin);
        bool inY = (y <= yMax && y >= yMin);
        bool inZ = (z <= zMax && z >= zMin);

        if (inX && inY && inZ)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(worldPosition, worldSize);
    }
}
