using System;
using System.Collections.Generic;
using UnityEngine;

// Simple movement planner tied to Map/Grid3<Cube>
[Serializable]
public class MovementPlanner
{
    private ModelActionContext _actionContext;

    [SerializeField] private Map map;
    [SerializeField] private int advanceRange = 0;
    [SerializeField] private int sprintRange = 0;

    public Map Map => map;

    public MovementPlanner(ModelActionContext modelActionContext)
    {
        _actionContext = modelActionContext;

        this.map = modelActionContext.Map;
        advanceRange = modelActionContext.SourceModel.unit.unitAdvanceSpeed;
        sprintRange = modelActionContext.SourceModel.unit.unitSprintSpeed;
    }


    // Returns reachable cubes (including origin) using 8-directional movement with cost 1 per cube.
    public List<Cube> GetReachableCubes(Cube origin, int range)
    {
        var result = new List<Cube>();
        if (map == null || origin == null || range < 0) return result;

        int ox = (int)origin.mapPosition.x;
        int oy = (int)origin.mapPosition.y;
        int oz = (int)origin.mapPosition.z;

        var visited = new HashSet<(int x, int y, int z)>();
        var q = new Queue<(int x, int y, int z, int dist)>();

        q.Enqueue((ox, oy, oz, 0));
        visited.Add((ox, oy, oz));
        result.Add(origin);

        while (q.Count > 0)
        {
            var node = q.Dequeue();
            if (node.dist >= range) continue;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;
                    int nx = node.x + dx;
                    int ny = node.y; // keep same height layer for now
                    int nz = node.z + dz;

                    if (visited.Contains((nx, ny, nz))) continue;
                    var neighbor = map.MapGrid.Get(nx, ny, nz);
                    if (neighbor == null) continue; // out of bounds or empty

                    visited.Add((nx, ny, nz));
                    result.Add(neighbor);
                    q.Enqueue((nx, ny, nz, node.dist + 1));
                }
            }
        }

        return result;
    }

    // Clamp a desired world point to the nearest point inside a reachable cube.
    // Preserves model vertical offset relative to its origin cube by default.
    public Vector3 ClampPointToRange(Cube origin, Vector3 desiredPoint, int range, float modelYOffset = 0f)
    {
        if (map == null || origin == null) return desiredPoint;

        var reachable = GetReachableCubes(origin, range);
        Debug.Log($"Reachable Cubes: {reachable.Count}");
        if (reachable == null || reachable.Count == 0) return desiredPoint;

        // 1) if desiredPoint is already inside a reachable cube -> clamp to cube interior and return
        foreach (var cube in reachable)
        {
            if (cube.PositionIsInCube(desiredPoint))
            {
                return ClampPointToCubeInterior(cube, desiredPoint, modelYOffset);
            }
        }

        // 2) otherwise find nearest reachable cube center (using XZ distance)
        Cube nearest = null;
        float bestDist = float.MaxValue;
        Vector2 pXZ = new Vector2(desiredPoint.x, desiredPoint.z);
        foreach (var cube in reachable)
        {
            var cXZ = new Vector2(cube.worldPosition.x, cube.worldPosition.z);
            float d = Vector2.SqrMagnitude(cXZ - pXZ);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = cube;
            }
        }

        if (nearest == null) nearest = origin;
        return ClampPointToCubeInterior(nearest, desiredPoint, modelYOffset);
    }

    private Vector3 ClampPointToCubeInterior(Cube cube, Vector3 desiredPoint, float modelYOffset)
    {
        var half = cube.worldSize * 0.5f;

        float minX = cube.worldPosition.x - half.x;
        float maxX = cube.worldPosition.x + half.x;
        float minY = cube.worldPosition.y - half.y;
        float maxY = cube.worldPosition.y + half.y;
        float minZ = cube.worldPosition.z - half.z;
        float maxZ = cube.worldPosition.z + half.z;

        float x = Mathf.Clamp(desiredPoint.x, minX, maxX);
        float z = Mathf.Clamp(desiredPoint.z, minZ, maxZ);

        // preserve a vertical offset (e.g. model's pivot offset) relative to the cube center Y
        float y = cube.worldPosition.y + modelYOffset;

        return new Vector3(x, y, z);
    }

    // Utility: find which cube contains a world point (search whole grid).
    public Cube GetCubeContainingPoint(Vector3 worldPoint)
    {
        for (int y = 0; y < map.MapSize.y; y++)
        {
            for (int x = 0; x < map.MapSize.x; x++)
            {
                for (int z = 0; z < map.MapSize.z; z++)
                {
                    var c = map.MapGrid.Get(x, y, z);
                    if (c == null) continue;
                    if (c.PositionIsInCube(worldPoint)) return c;
                }
            }
        }
        return null;
    }
}