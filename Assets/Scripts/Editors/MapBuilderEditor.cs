using Assets.Scripts;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    SerializedProperty map;

    public Map Map
    {
        get => (Map)map.GetUnderlyingValue();
        set => map.SetUnderlyingValue(value);
    }

    public void OnEnable()
    {
        map = serializedObject.FindProperty("map");
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();


        if (GUILayout.Button("Create New Map"))
        {
            //add everthing the button would do.
            var newMap = new Map
            {
                MapName = "New Map",
                MapSize = new Vector3IntS(8,8,8),
                CubeSize = 76.2f,
                Terrain = new Terrain[0],
                MapGrid = new Grid3<Cube>(8, 8, 8)
            };
            Debug.Log($"newMap: {newMap.MapName}");
            Map = newMap;
        }
        if (GUILayout.Button("Load Map from File"))
        {
            //add everthing the button would do.
            ((MapBuilder)target)
                .BuildMap()
                .SpawnGroundPlane()
                .SpawnGridLines()
                .SpawnTerrain()
                .DrawDeploymentZones();
        }
        if (GUILayout.Button("Create Ground Plane")) {
            ((MapBuilder)target)
                .SpawnGroundPlane ();
        }

        if (GUILayout.Button("Save Map"))
        {
            // clear current terrain from the map to avoid dupes
            Map.Terrain = new Terrain[0];

            // add the terrain currently a child of this obeserved object
            var terrain = ((MapBuilder)target).GetComponentsInChildren<Terrain>();
            Debug.Log(terrain.Length);
            foreach (var t in terrain)
            {
                t.SetPositionFromTransform (t.transform);
                t.SetRotationFromTransform (t.transform);

                Map.AddTerrain(t);
            }

            Map.Save(Application.dataPath + "/Maps/" + Map.MapName + ".json");
            Debug.Log("Map Saved.");
        }

        if (GUILayout.Button("Clear Terrain"))
        {
            var mapObjects = ((MapBuilder)target).GetComponentsInChildren<Terrain>(includeInactive: true);
            for (int i = mapObjects.Length; i > 0; --i)
                DestroyImmediate(mapObjects[i].gameObject);
        }
        if (GUILayout.Button("Reset Map Builder"))
        {
            var mapObjects = ((MapBuilder)target).GetComponentInChildren<Transform>(includeInactive: true);
            for (int i = mapObjects.transform.childCount; i > 0; --i)
                DestroyImmediate(mapObjects.transform.GetChild(0).gameObject);


            map.SetUnderlyingValue(null);
        }
    }

}