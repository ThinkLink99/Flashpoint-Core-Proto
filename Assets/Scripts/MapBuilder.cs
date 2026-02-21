using UnityEngine;

namespace Assets.Scripts
{
    public class MapBuilder : MonoBehaviour
    {
        [SerializeField] private TextAsset mapJson;
        [SerializeField] private Material groundMaterial;
        [SerializeField] private TerrainSetScriptableObject terrainSet;
        [SerializeField] private bool showDebugLogs = true;

        [SerializeField] private GameEvent onMapCreated;

        public void BuildMap()
        {
            if (showDebugLogs) Debug.Log("Loading map...");
            // use the map data to build the map in the scene
            var map = Map.Load(mapJson);
            map.MapGrid = new Grid3<Cube>(map.MapSize.X, map.MapSize.Y, map.MapSize.Z);

            if (showDebugLogs) Debug.Log("Map loaded.");
            if (showDebugLogs) Debug.Log($"Map Name: {map.MapName}");
            if (showDebugLogs) Debug.Log($"Map Size: {map.MapSize.ToString()}");

            if (showDebugLogs) Debug.Log("Creating Ground Plane...");
            // instantiate a ground plane and set its scale to match the map size
            var mesh = CreatePlane("Materials/testGround", "Ground", map.MapSize.X * map.CubeSize, map.MapSize.Z * map.CubeSize);
            //var plane = Instantiate<GameObject>(mesh, this.transform);
            mesh.transform.position = new Vector3((map.MapSize.X * map.CubeSize) / 2, 0, (map.MapSize.Z * map.CubeSize) / 2);
            mesh.transform.localScale = new Vector3(1, 1, 1);
            if (showDebugLogs) Debug.Log("Ground Plane Created.");

            if (showDebugLogs) Debug.Log("Placing Terrain pieces...");
            var start = this.transform.localPosition;
            for (int y = 0; y < map.MapSize.Y; y++)
            {
                for (int x = 0; x < map.MapSize.X; x++)
                {
                    for (int z = 0; z < map.MapSize.Z; z++)
                    {
                        var terrain = Instantiate(terrainSet.terrainPieces[map.Layers[y].Objects[x, z].Type]);
                        terrain.transform.localPosition = new Vector3((x * terrain.transform.lossyScale.x), (y * terrain.transform.lossyScale.y) + (terrain.transform.lossyScale.y / 2), (z * terrain.transform.lossyScale.z));
                        if (showDebugLogs) Debug.Log($"Placing {terrain.name}");

                        var cube = terrain.AddComponent<Cube>();
                        cube.mapPosition = new Vector3(x, y, z);
                        cube.worldPosition = terrain.transform.localPosition;
                        cube.worldSize = terrain.transform.lossyScale;

                        map.MapGrid.Add(cube, x, y, z);
                    }
                }
            }

            if (showDebugLogs) Debug.Log("Terrain pieces placed.");
            onMapCreated?.Raise(this, map);
        }

        private GameObject CreatePlane(string TexturePath, string Name, float Width, float Height)
        {
            GameObject g = new GameObject(Name);
            g.transform.localEulerAngles = new Vector3(0, 0, 0);
            g.AddComponent<MeshFilter>();
            g.AddComponent<MeshRenderer>();
            g.GetComponent<MeshFilter>().mesh = CreatePlaneMesh(Width, Height);
            g.AddComponent<MeshCollider>().sharedMesh = g.GetComponent<MeshFilter>().mesh;

            g.GetComponent<MeshRenderer>().material = groundMaterial;
            //Material m = new Material("my_material");
            //m.name = "my_material";
            ////m.shader = Shader.Find("Transparent/Cutout/Diffuse");
            //m.shader = Shader.Find("Somian/Unlit/Transparent");

            //g.material = m;
            //g.renderer.material.mainTexture = (Texture2D)Resources.Load(TexturePath);
            //g.renderer.castShadows = false;
            //g.renderer.receiveShadows = false;

            return g;
        }
        private Mesh CreatePlaneMesh(float Width, float Height)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[] { new Vector3(Width, 0, Height), new Vector3(Width, 0, -Height), new Vector3(-Width, 0, Height), new Vector3(-Width, 0, -Height) };
            Vector2[] uv = new Vector2[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0) };
            int[] triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            mesh.vertices = vertices;
            mesh.uv = uv;


            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}