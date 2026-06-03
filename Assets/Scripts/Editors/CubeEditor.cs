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
        }
        if (GUILayout.Button("Check Space Inside"))
        {
            Cube cube = (Cube)target;
            bool hasSpace = cube.HasSufficientSpace();
            Debug.Log($"Cube at {cube.mapPosition} has sufficient space inside: {hasSpace}");
        }
    }
}