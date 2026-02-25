using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class MovementPlanner
{
    private readonly Map map;

    public MovementPlanner(Map map) { this.map = map; }

    // BFS on grid neighbors (6-directional). returns set of reachable cubes including origin.
    public List<Cube> GetReachableCubes(Cube origin, int steps)
    {
        var result = new List<Cube>();
        if (origin == null || map == null) return result;

        var visited = new HashSet<(int x, int y, int z)>();
        var q = new Queue<(Cube cube, int depth)>();
        int ox = Mathf.RoundToInt(origin.mapPosition.x);
        int oy = Mathf.RoundToInt(origin.mapPosition.y);
        int oz = Mathf.RoundToInt(origin.mapPosition.z);

        q.Enqueue((origin, 0));
        visited.Add((ox, oy, oz));

        while (q.Count > 0)
        {
            var (cube, depth) = q.Dequeue();
            result.Add(cube);

            if (depth >= steps) continue;

            // neighbor offsets in 6 directions
            var neighbors = new (int dx, int dy, int dz)[]
            {
                (1,0,0), (-1,0,0), (0,1,0), (0,-1,0), (0,0,1), (0,0,-1)
            };

            int cx = Mathf.RoundToInt(cube.mapPosition.x);
            int cy = Mathf.RoundToInt(cube.mapPosition.y);
            int cz = Mathf.RoundToInt(cube.mapPosition.z);

            foreach (var n in neighbors)
            {
                int nx = cx + n.dx;
                int ny = cy + n.dy;
                int nz = cz + n.dz;
                if (visited.Contains((nx, ny, nz))) continue;
                var neighborCube = map.MapGrid.Get(nx, ny, nz);
                if (neighborCube == null) continue;

                // simple walkability check: you can extend this to check obstacles
                visited.Add((nx, ny, nz));
                q.Enqueue((neighborCube, depth + 1));
            }
        }

        return result;
    }

    // Clamp a world point to the furthest cube within `steps` and snap to that cube's world position when possible.
    public Vector3 ClampPointToRange(Cube origin, Vector3 desiredWorldPoint, int steps)
    {
        if (origin == null || map == null) return desiredWorldPoint;

        float cubeSize = map.CubeSize;
        float maxDistanceWorld = steps * cubeSize;
        Vector3 originWorld = origin.worldPosition;
        float dist = Vector3.Distance(originWorld, desiredWorldPoint);

        if (dist <= maxDistanceWorld) return desiredWorldPoint;

        Vector3 clamped = originWorld + (desiredWorldPoint - originWorld).normalized * maxDistanceWorld;

        // snap to nearest cube if exists
        var snapped = GetCubeAtWorldPosition(clamped);
        return snapped != null ? snapped.worldPosition : clamped;
    }

    // Find cube in map that contains world position (linear search similar to previous implementation)
    private Cube GetCubeAtWorldPosition(Vector3 position)
    {
        for (int x = 0; x < map.MapSize.x; x++)
        {
            for (int y = 0; y < map.MapSize.y; y++)
            {
                for (int z = 0; z < map.MapSize.z; z++)
                {
                    var cube = map.MapGrid.Get(x, y, z);
                    if (cube is null) continue;

                    if (cube.PositionIsInCube(position))
                    {
                        return cube;
                    }
                }
            }
        }

        return null;
    }
}
