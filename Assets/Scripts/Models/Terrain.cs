using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Collider terrainCollider;
    [SerializeField] private List<Cube> cubesIn;
    [SerializeField] private Tabletop tabletop;
    [SerializeField] private Vector3[] vertices;

    [Header("Debugging")]
    [SerializeField] private bool showDebugMessages = true;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Vector3 lastPosition = Vector3.positiveInfinity;

    private void Awake()
    {
        if (terrainMesh == null) terrainMesh = GetComponentInChildren<MeshFilter>();
        if (terrainCollider == null) terrainCollider = GetComponentInChildren<Collider>();
        vertices = terrainMesh.mesh.vertices;

        if (transform.parent != null)
        {
            tabletop = transform.parent.GetComponentInParent<Tabletop>();
        }
    }
    private void OnValidate()
    {
        if (terrainMesh == null) terrainMesh = GetComponentInChildren<MeshFilter>();
        if (terrainCollider == null) terrainCollider = GetComponentInChildren<Collider>();
        vertices = terrainMesh.sharedMesh.vertices; 
    }
    private void Update()
    {
        if (lastPosition != transform.position)
        {
            GetCubesIn();
            lastPosition = transform.position;
        }
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

        if (tabletop == null || tabletop.currentMap == null)
        {
            if (showDebugMessages) Debug.LogWarning("Tabletop or CurrentMap is null. Cannot compute cubes containing terrain.");
            return;
        }

        // Prefer physics overlap test if the terrain has a Collider (recommended: add a MeshCollider to the terrain)
        if (terrainCollider != null)
        {
            // Iterate all cubes and check if the cube's world box overlaps the terrain collider
            for (int y = 0; y < tabletop.currentMap.MapSize.y; y++)
            {
                for (int x = 0; x < tabletop.currentMap.MapSize.x; x++)
                {
                    for (int z = 0; z < tabletop.currentMap.MapSize.z; z++)
                    {
                        var c = tabletop.currentMap.MapGrid.Get(x, y, z);
                        if (c == null) continue;

                        // Use the cube's center and half extents for overlap test
                        Vector3 halfExtents = c.worldSize * 0.5f;
                        // Slight epsilon to catch thin intersections
                        halfExtents += Vector3.one * 0.001f;

                        // OverlapBox will return any colliders overlapping the cube box
                        Collider[] hits = Physics.OverlapBox(c.worldPosition, halfExtents, Quaternion.identity);
                        bool intersectsTerrain = false;
                        for (int i = 0; i < hits.Length; i++)
                        {
                            var hit = hits[i];
                            if (hit == null) continue;
                            // match the terrain collider itself or any child collider of the terrain object
                            if (hit == terrainCollider || hit.transform.IsChildOf(terrainCollider.transform))
                            {
                                intersectsTerrain = true;
                                break;
                            }
                        }

                        if (intersectsTerrain && !cubesIn.Contains(c))
                        {
                            if (showDebugMessages) Debug.Log($"Terrain intersects cube: {c.worldPosition}");
                            c.isPassable = false;
                            cubesIn.Add(c);
                        }
                    }
                }
            }
            return;
        }

        //// Fallback: original vertex-based approach (less reliable)
        //// loop through the vertexes of a hidden simpler shape and see if they fall within a cube
        //foreach (var vertex in vertices) 
        //{
        //    var transformedVertex = transform.TransformPoint(vertex);
        //    //var transformedVertex = GetVertexWorldPosition(vertex, transform);
        //    if (showDebugMessages) Debug.Log($"Terrain has local Position: {transform.position}");
        //    //var transformedVertex = vertex + transform.position;

        //    if (showDebugMessages) Debug.Log($"Checking cube for vertex: V({vertex}) T({transformedVertex})");
        //    var cubes = GetCubeContainingPoint(transformedVertex);
        //    foreach (var cube in cubes)
        //    {
        //        if (showDebugMessages) Debug.Log($"Cube found: {cube.worldPosition}");
        //        if (!cubesIn.Contains(cube))
        //        {
        //            if (showDebugMessages) Debug.Log($"Cube already in list: {cube.worldPosition}");
        //            cubesIn.Add(cube);
        //        }
        //    }
        //}
    }
    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    {
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    }

    private IEnumerable<Cube> GetCubeContainingPoint(Vector3 worldPoint)
    {
        var cubes = new List<Cube>();
        for (int y = 0; y < tabletop.currentMap.MapSize.y; y++)
        {
            for (int x = 0; x < tabletop.currentMap.MapSize.x; x++)
            {
                for (int z = 0; z < tabletop.currentMap.MapSize.z; z++)
                {
                    var c = tabletop.currentMap.MapGrid.Get(x, y, z);
                    if (c == null) continue;
                    if (c.PositionIsInCube(worldPoint) && !cubes.Contains(c)) cubes.Add(c);
                }
            }
        }
        return cubes;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = Color.cyan;
        foreach (var cube in cubesIn)
        {
            Gizmos.DrawWireCube(cube.worldPosition, cube.worldSize);
        }
    }
}
