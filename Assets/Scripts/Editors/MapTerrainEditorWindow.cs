using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editors
{
    public class MapTerrainEditorWindow : EditorWindow
    {
        private MapBuilder mapBuilder;
        private Map map;
        private TerrainSetScriptableObject terrainSet;

        [MenuItem("Tools/Terrain Editor")]
        public static void ShowWindow()
        {
            var w = GetWindow<MapTerrainEditorWindow>("Terrain");
            w.minSize = new Vector2(300, 220);
        }
    }
}