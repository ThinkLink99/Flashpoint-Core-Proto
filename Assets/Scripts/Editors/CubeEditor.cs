using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cube))]
public class CubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Check Ground Below"))
        {
            Cube cube = (Cube)target;
            bool hasGround = cube.HasSufficientGround();
            Debug.Log($"Cube at {cube.mapPosition} has sufficient ground below: {hasGround}");
            Time.timeScale = 0.1f; // Slow down time to better observe the results
        }
    }
}