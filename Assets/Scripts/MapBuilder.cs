using UnityEngine;

namespace Assets.Scripts
{
    public class MapBuilder : MonoBehaviour
    {
        [SerializeField] private Map map;
        [SerializeField] private TextAsset mapJson;
        [SerializeField] private GameObject emptyCube;
        [SerializeField] private Material groundMaterial;
        [SerializeField] private TerrainSetScriptableObject terrainSet;
        [SerializeField] private bool showDebugLogs = true;

        [SerializeField] private GameEvent onMapCreated;

        public void OnGameStart (Component sender, object data)
        {
            BuildMap()
            .SpawnGroundPlane()
            .SpawnGridLines()
            .SpawnTerrain()
            .DrawDeploymentZones()
            .RaiseMapCreatedEvent ();
        }

        public MapBuilder BuildMap()
        {
            if (showDebugLogs) Debug.Log("Loading map...");
            // use the map data to build the map in the scene
            map = Map.Load(mapJson);
            map.MapGrid = new Grid3<Cube>(map.MapSize.x, map.MapSize.y, map.MapSize.z);

            if (showDebugLogs) Debug.Log("Map loaded.");
            if (showDebugLogs) Debug.Log($"Map Name: {map.MapName}");
            if (showDebugLogs) Debug.Log($"Map Size: {map.MapSize.ToString()}");

            onMapCreated?.Raise(this, map);
            return this;
        }
        public MapBuilder SpawnGroundPlane()
        {
            if (showDebugLogs) Debug.Log("Creating Ground Plane...");
            // instantiate a ground plane and set its scale to match the map size
            var mesh = CreatePlane("Materials/testGround", "Ground", map.MapSize.x * map.CubeSize, map.MapSize.z * map.CubeSize);
            //var plane = Instantiate<GameObject>(mesh, this.transform);
            mesh.transform.parent = this.transform;
            mesh.transform.position = new Vector3((map.MapSize.x * map.CubeSize) / 2, 0, (map.MapSize.z * map.CubeSize) / 2);
            mesh.transform.localScale = new Vector3(1, 1, 1);
            if (showDebugLogs) Debug.Log("Ground Plane Created.");
            return this;
        }
        public MapBuilder SpawnGridLines()
        {
            if (showDebugLogs) Debug.Log("Spawning Grid Lines...");
            // instantiate grid lines based on the map size and cube size

            for (int y = 0; y < map.MapSize.y; y++)
            {
                for (int x = 0; x < map.MapSize.x; x++)
                {
                    for (int z = 0; z < map.MapSize.z; z++)
                    {
                        var terrain = Instantiate(emptyCube, this.transform);
                        terrain.transform.localPosition = new Vector3((x * terrain.transform.lossyScale.x), (y * terrain.transform.lossyScale.y) + (terrain.transform.lossyScale.y / 2), (z * terrain.transform.lossyScale.z));
                        if (showDebugLogs) Debug.Log($"Placing {terrain.name}");

                        var cube = terrain.GetComponent<Cube>();
                        cube.mapPosition = new Vector3(x, y, z);
                        cube.worldPosition = terrain.transform.localPosition;
                        cube.worldSize = terrain.transform.lossyScale;

                        map.MapGrid.Add(cube, x, y, z);
                    }
                }
            }

            if (showDebugLogs) Debug.Log("Grid Lines Spawned.");
            return this;
        }
        public MapBuilder SpawnTerrain() 
        {
            if (showDebugLogs) Debug.Log("Placing Terrain pieces...");
            var start = this.transform.localPosition;
            
            for (int x = 0; x < map.Terrain.Length; x++)
            {
                var terrain = Instantiate(terrainSet.terrainPieces[map.Terrain[x].id], this.transform);
                terrain.transform.localPosition = map.Terrain[x].worldPositon.ToVector3();
                terrain.transform.localEulerAngles = map.Terrain[x].worldRotation.ToVector3();
                if (showDebugLogs) Debug.Log($"Placing {terrain.name}");
            }

            if (showDebugLogs) Debug.Log("Terrain pieces placed.");
            return this;
        }
        public MapBuilder DrawDeploymentZones ()
        {
            if (showDebugLogs) Debug.Log("Drawing Deployment Zones...");


            if (showDebugLogs) Debug.Log("Deployment Zones Drawn.");
            return this;
        }
        public void RaiseMapCreatedEvent ()
        {
            onMapCreated?.Raise(this, map);
        }


        private GameObject CreatePlane(string TexturePath, string Name, float Width, float Height)
        {
            GameObject g = new GameObject(Name);
            g.transform.localEulerAngles = new Vector3(0, 0, 0);
            g.AddComponent<MeshFilter>();
            g.AddComponent<MeshRenderer>();
            g.GetComponent<MeshFilter>().sharedMesh = CreatePlaneMesh(Width, Height);
            g.AddComponent<MeshCollider>().sharedMesh = g.GetComponent<MeshFilter>().sharedMesh;

            g.GetComponent<MeshRenderer>().material = new Material(groundMaterial);
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