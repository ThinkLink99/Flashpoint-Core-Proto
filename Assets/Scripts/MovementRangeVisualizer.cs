using System.Collections.Generic;
using UnityEngine;

public class MovementRangeVisualizer : MonoBehaviour
{
    private LineRenderer rangeRenderer;
    private int circleSegments = 64;

    private GameObject movementPreviewRoot;
    private readonly List<GameObject> movementPreviewMarkers = new List<GameObject>();

    public void ShowRangeCircle(Vector3 center, float radius)
    {
        EnsureRangeRenderer();
        for (int i = 0; i <= circleSegments; i++)
        {
            float t = i / (float)circleSegments;
            float ang = t * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius) + center;
            rangeRenderer.SetPosition(i, p);
        }
        rangeRenderer.gameObject.SetActive(true);
    }

    public void HideRangeCircle()
    {
        if (rangeRenderer != null) rangeRenderer.gameObject.SetActive(false);
    }

    public void ShowMarkers(IEnumerable<Cube> cubes)
    {
        ClearMarkers();
        if (cubes == null) return;

        movementPreviewRoot = new GameObject("MovementPreviewRoot");
        movementPreviewRoot.transform.SetParent(this.transform, false);

        Material markerMat = new Material(Shader.Find("Sprites/Default")) { color = new Color(0f, 0.8f, 0f, 0.35f) };

        foreach (var cube in cubes)
        {
            if (cube == null) continue;
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"Preview_{cube.mapPosition.x}_{cube.mapPosition.y}_{cube.mapPosition.z}";
            marker.transform.SetParent(movementPreviewRoot.transform, true);
            marker.transform.position = cube.worldPosition + new Vector3(0f, 0.02f, 0f);
            float markerScale = Mathf.Max(0.01f, Mathf.Min(cube.worldSize.x, cube.worldSize.z)) * 0.8f;
            marker.transform.localScale = new Vector3(markerScale, 0.02f, markerScale);
            var rend = marker.GetComponent<Renderer>();
            rend.sharedMaterial = markerMat;
            var col = marker.GetComponent<Collider>();
            if (col != null) Destroy(col);

            movementPreviewMarkers.Add(marker);
        }
    }

    public void ClearMarkers()
    {
        for (int i = movementPreviewMarkers.Count - 1; i >= 0; i--)
        {
            if (movementPreviewMarkers[i] != null) Destroy(movementPreviewMarkers[i]);
        }
        movementPreviewMarkers.Clear();

        if (movementPreviewRoot != null)
        {
            Destroy(movementPreviewRoot);
            movementPreviewRoot = null;
        }
    }

    private void EnsureRangeRenderer()
    {
        if (rangeRenderer != null) return;
        var go = new GameObject("RangeRenderer");
        go.transform.SetParent(transform);
        rangeRenderer = go.AddComponent<LineRenderer>();
        rangeRenderer.positionCount = circleSegments + 1;
        rangeRenderer.loop = true;
        rangeRenderer.useWorldSpace = true;
        rangeRenderer.startWidth = 0.05f;
        rangeRenderer.endWidth = 0.05f;
        rangeRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1f, 1f, 1f, 0.25f) };
    }
}
