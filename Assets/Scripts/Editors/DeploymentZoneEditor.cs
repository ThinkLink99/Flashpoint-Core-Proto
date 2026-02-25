using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Editors
{
    public class DeploymentZoneEditor : EditorWindow
    {
        private MapBuilder mapBuilder;
        private Map map;
        private int selectedZoneIndex = -1;
        private bool paintMode = false;
        private Vector2 scroll;
        private GUIStyle centered;
        private bool autoSelectMapBuilder = true;

        [MenuItem("Tools/Deployment Zone Editor")]
        public static void ShowWindow()
        {
            var w = GetWindow<DeploymentZoneEditor>("Deployment Zones");
            w.minSize = new Vector2(300, 220);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            centered = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            OnHierarchyChanged();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnHierarchyChanged()
        {
            if (!autoSelectMapBuilder) return;
            if (mapBuilder == null)
            {
                var mb = FindObjectOfType<MapBuilder>();
                if (mb != null)
                    SetMapBuilder(mb);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Deployment Zone Editor", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MapBuilder", GUILayout.Width(80));
            var newBuilder = (MapBuilder)EditorGUILayout.ObjectField(mapBuilder, typeof(MapBuilder), true);
            if (newBuilder != mapBuilder)
            {
                SetMapBuilder(newBuilder);
                autoSelectMapBuilder = false;
            }
            if (GUILayout.Button("Select In Scene", GUILayout.Width(110)))
            {
                if (mapBuilder != null)
                    Selection.activeGameObject = mapBuilder.gameObject;
            }
            EditorGUILayout.EndHorizontal();

            if (mapBuilder == null)
            {
                EditorGUILayout.HelpBox("Assign a MapBuilder from the scene to edit deployment zones.", MessageType.Info);
                return;
            }

            if (map == null)
            {
                EditorGUILayout.HelpBox("Selected MapBuilder has no Map instance (private `map` field is null). Build or load a map first.", MessageType.Warning);
                if (GUILayout.Button("Build/Load Map in MapBuilder"))
                {
                    mapBuilder.BuildMap();
                }
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Zone"))
            {
                CreateNewZone();
            }
            if (GUILayout.Button("Delete Zone"))
            {
                DeleteSelectedZone();
            }
            if (GUILayout.Button(paintMode ? "Exit Paint Mode" : "Enter Paint Mode"))
            {
                paintMode = !paintMode;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < map.DeploymentZones.Count; i++)
            {
                var zone = map.DeploymentZones[i];
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Toggle(i == selectedZoneIndex, $"Zone {i} - Team: {zone.teamId}  Squares: {zone.squares?.Count ?? 0}", "Button"))
                {
                    selectedZoneIndex = i;
                }
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    zone.squares = new List<Vector2IntS>();
                    SaveMapToBuilder();
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Instructions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(" - Enter paint mode, then click cubes in Scene view to toggle their inclusion in the selected zone.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField(" - If cubes have no pickable renderer, the tool will fallback to nearest-cube picking by X,Z.", EditorStyles.wordWrappedLabel);

            if (GUI.changed) Repaint();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!paintMode || mapBuilder == null || map == null || selectedZoneIndex < 0 || selectedZoneIndex >= map.DeploymentZones.Count)
                return;

            var e = Event.current;
            Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            // 1) Try a physics raycast that includes trigger colliders (handles trigger-only cubes).
            Cube hovered = null;
            if (Physics.Raycast(worldRay, out RaycastHit hit, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                hovered = hit.collider.GetComponent<Cube>();
            }

            // 2) Fallback: if no collider hit, find nearest cube by projecting cube world positions to the ray.
            if (hovered == null)
            {
                hovered = PickNearestCubeByRay(worldRay, maxDistanceThreshold: 0.6f);
            }

            if (hovered != null)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireCube(hovered.worldPosition, hovered.worldSize);
                SceneView.RepaintAll();

                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt) // left click (ignore alt orbit)
                {
                    Debug.Log($"Toggling cube at {hovered.mapPosition} in zone {selectedZoneIndex}");
                    ToggleCubeInSelectedZone(hovered);
                    e.Use();
                }
            }
        }

        // Ray-to-cube proximity pick. Returns nearest cube whose X,Z center is within a tolerance of the ray.
        private Cube PickNearestCubeByRay(Ray ray, float maxDistanceThreshold = 0.5f)
        {
            if (map.MapGrid == null) return null;

            Cube best = null;
            float bestDist = float.MaxValue;

            int sizeX = map.MapSize.x;
            int sizeY = map.MapSize.y;
            int sizeZ = map.MapSize.y == 0 ? map.MapSize.z : map.MapSize.z; // defensive

            // iterate XYXZ bounds; pick representative cube per X,Z (first non-null across Y)
            for (int x = 0; x < map.MapSize.x; x++)
            {
                for (int z = 0; z < map.MapSize.z; z++)
                {
                    Cube sample = null;
                    for (int y = 0; y < map.MapSize.y; y++)
                    {
                        var c = map.MapGrid.Get(x, y, z);
                        if (c != null)
                        {
                            sample = c;
                            break;
                        }
                    }
                    if (sample == null) continue;

                    // distance from cube center to ray
                    float dist = DistancePointToRay(sample.worldPosition, ray);
                    // tolerance scaled to cube size (use larger of X/Z extents)
                    float tolerance = Mathf.Max(sample.worldSize.x, sample.worldSize.z) * maxDistanceThreshold;
                    if (dist <= tolerance && dist < bestDist)
                    {
                        bestDist = dist;
                        best = sample;
                    }
                }
            }

            return best;
        }

        private float DistancePointToRay(Vector3 point, Ray ray)
        {
            Vector3 toPoint = point - ray.origin;
            float t = Vector3.Dot(toPoint, ray.direction.normalized);
            Vector3 projected = ray.origin + Mathf.Max(t, 0f) * ray.direction.normalized;
            return Vector3.Distance(point, projected);
        }

        private void ToggleCubeInSelectedZone(Cube cube)
        {
            var pos = cube.mapPosition;
            int x = Mathf.RoundToInt(pos.x);
            int z = Mathf.RoundToInt(pos.z);

            var zone = map.DeploymentZones[selectedZoneIndex];
            if (zone.squares == null) zone.squares = new List<Vector2IntS>();

            var existing = zone.squares.FirstOrDefault(s => s.x == x && s.y == z);
            if (existing != null && zone.squares.Remove(existing))
            {
                // removed
                Debug.Log($"Removed cube at {x},{z} from zone {selectedZoneIndex}");
            }
            else
            {
                Debug.Log($"Added cube at {x},{z} to zone {selectedZoneIndex}");
                zone.squares.Add(new Vector2IntS(x, z));
            }

            map.DeploymentZones[selectedZoneIndex] = zone; // ensure list reference is updated

            SaveMapToBuilder();
            EditorUtility.SetDirty(mapBuilder);
            EditorSceneManager.MarkSceneDirty(mapBuilder.gameObject.scene);
            Repaint();
        }

        private void CreateNewZone()
        {
            var dz = new DeploymentZone { teamId = TeamId.Red, squares = new List<Vector2IntS>() };
            map.DeploymentZones.Add(dz);
            selectedZoneIndex = map.DeploymentZones.Count - 1;
            SaveMapToBuilder();
        }

        private void DeleteSelectedZone()
        {
            if (selectedZoneIndex < 0 || selectedZoneIndex >= map.DeploymentZones.Count) return;
            if (!EditorUtility.DisplayDialog("Delete Zone", $"Delete zone {selectedZoneIndex}?", "Delete", "Cancel")) return;
            map.DeploymentZones.RemoveAt(selectedZoneIndex);
            selectedZoneIndex = Mathf.Clamp(selectedZoneIndex - 1, 0, map.DeploymentZones.Count - 1);
            SaveMapToBuilder();
        }

        private void SetMapBuilder(MapBuilder mb)
        {
            mapBuilder = mb;
            map = null;
            if (mapBuilder == null) return;

            var field = typeof(MapBuilder).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                map = field.GetValue(mapBuilder) as Map;
            }

            if (map == null)
            {
                try
                {
                    mapBuilder.BuildMap();
                    map = field.GetValue(mapBuilder) as Map;
                }
                catch { }
            }

            if (map != null && map.DeploymentZones == null)
                map.DeploymentZones = new List<DeploymentZone>();
        }

        private void SaveMapToBuilder()
        {
            if (mapBuilder == null) return;
            var field = typeof(MapBuilder).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                Debug.Log("Saving deployment zones to MapBuilder's private `map` field");
                field.SetValue(mapBuilder, map);
                EditorUtility.SetDirty(mapBuilder);
                EditorSceneManager.MarkSceneDirty(mapBuilder.gameObject.scene);
            }
        }
    }
}