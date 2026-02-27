using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Terrain : MonoBehaviour
{
    [JsonProperty("id")]
    public string id;
    [JsonProperty("world_position")]
    public Vector3S worldPositon;
    [JsonProperty("world_rotation")]
    public Vector3S worldRotation;

    [SerializeField] private MeshFilter terrainMesh;
    [SerializeField] private List<Cube> cubesIn;
    [SerializeField] private Tabletop tabletop;
    [SerializeField] private Vector3[] vertices;

    private void Awake()
    {
        if (terrainMesh == null) terrainMesh = GetComponentInChildren<MeshFilter>();
        vertices = terrainMesh.sharedMesh.vertices;

        if (transform.parent != null)
        {
            tabletop = transform.parent.GetComponentInParent<Tabletop>();
            GetCubesIn();
        }
    }
    private void OnValidate()
    {
        if (terrainMesh == null) terrainMesh = GetComponentInChildren<MeshFilter>();
        vertices = terrainMesh.sharedMesh.vertices; 
    }

    public void SetPositionFromTransform (Transform transform)
    {
        worldPositon = new Vector3S(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }
    public void SetRotationFromTransform(Transform transform)
    {
        worldRotation = new Vector3S(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    private void GetCubesIn ()
    {
        cubesIn.Clear();

        // loop through the vertexes of a hidden simpler shape and see if they fall within a cube
        foreach (var vertex in vertices)
        {
            Debug.Log($"Checking cube for vertex: {vertex}");
            var cube = GetCubeContainingPoint(vertex);
            if (cube != null)
            {
                Debug.Log($"Cube found: {cube.worldPosition}");
                if (!cubesIn.Contains(cube))
                {
                Debug.Log($"Cube already in list: {cube.worldPosition}");
                    cubesIn.Add(cube);
                }
            }
        }
    }

    private Cube GetCubeContainingPoint(Vector3 worldPoint)
    {
        for (int y = 0; y < tabletop.CurrentMap.MapSize.y; y++)
        {
            for (int x = 0; x < tabletop.CurrentMap.MapSize.x; x++)
            {
                for (int z = 0; z < tabletop.CurrentMap.MapSize.z; z++)
                {
                    var c = tabletop.CurrentMap.MapGrid.Get(x, y, z);
                    if (c == null) continue;
                    if (c.PositionIsInCube(worldPoint)) return c;
                }
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var cube in cubesIn)
        {
            Gizmos.DrawWireCube(cube.worldPosition, cube.worldSize);
        }
    }
}
