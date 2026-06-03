using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Simple movement planner tied to Map/Grid3<Cube>
[Serializable]
public class MovementPlanner
{
    private GameActionContext _actionContext;

    [SerializeField] private Map map;
    [SerializeField] private int advanceRange = 0;
    [SerializeField] private int sprintRange = 0;

    public Map Map => map;

    public MovementPlanner(GameActionContext modelActionContext)
    {
        _actionContext = modelActionContext;

        this.map = modelActionContext.Map;
        advanceRange = modelActionContext.SourceModel.ModelConfiguration.unitAdvanceSpeed;
        sprintRange = modelActionContext.SourceModel.ModelConfiguration.unitSprintSpeed;
    }


    // Respects Cube.isPassable and prevents diagonal corner-cutting.
    public List<Cube> GetReachableCubes(Cube origin, int range)
    {
        var result = new List<Cube>();
        if (map == null || origin == null || range < 0) return result;

        int origin_x = (int)origin.mapPosition.x;
        int origin_y = (int)origin.mapPosition.y;
        int origin_z = (int)origin.mapPosition.z;

        var visited = new HashSet<(int x, int y, int z)>();  
        var q = new Queue<(int x, int y, int z, int dist)>();

        // origin always starts in queue/result (so unit can stay in place even if origin flagged not passable)
        q.Enqueue((origin_x, origin_y, origin_z, 0));
        visited.Add((origin_x, origin_y, origin_z));
        result.Add(origin);

        while (q.Count > 0)
        {
            var node = q.Dequeue();
            if (node.dist >= range) continue;

            for (int delta_y = -1; delta_y <= 1; delta_y++)
            {
                for (int delta_x = -1; delta_x <= 1; delta_x++)
                {
                    for (int delta_z = -1; delta_z <= 1; delta_z++)
                    {
                        if (delta_x == 0 && delta_y == 0 && delta_z == 0) continue; // skip the "no movement" case only

                        int neighbor_x = node.x + delta_x;
                        int neighbor_y = node.y + delta_y;
                        int neighbor_z = node.z + delta_z;

                        // we have already checked this cube, skip it.
                        if (visited.Contains((neighbor_x, neighbor_y, neighbor_z))) continue;

                        var neighbor = map.MapGrid.Get(neighbor_x, neighbor_y, neighbor_z);
                        if (neighbor == null) continue; // out of bounds

                        // Skip impassable cubes
                        if (!neighbor.isPassable) continue;
                        if (!neighbor.hasTerrainBelow) continue;

                        // Prevent diagonal corner cutting in XZ plane:
                        // if movement is diagonal in XZ, ensure the two orthogonal steps at the target Y are passable.
                        if (delta_x != 0 && delta_z != 0)
                        {
                            var orthX = map.MapGrid.Get(node.x + delta_x, node.y + delta_y, node.z);   // step in X at target Y
                            var orthZ = map.MapGrid.Get(node.x, node.y + delta_y, node.z + delta_z);   // step in Z at target Y

                            if (orthX == null || orthZ == null) continue;
                            if (!orthX.isPassable || !orthZ.isPassable) continue;
                        }

                        visited.Add((neighbor_x, neighbor_y, neighbor_z));
                        result.Add(neighbor);
                        q.Enqueue((neighbor_x, neighbor_y, neighbor_z, node.dist + 1));
                    }
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

    public bool IsSprintMove (Cube origin, Vector3 worldPoint)
    {
        var cube = GetCubeContainingPoint(worldPoint);
        if (cube == null) return false;

        var sprintRangeCubes = GetReachableCubes(origin, sprintRange);
        var advanceRangeCubes = GetReachableCubes(origin, advanceRange);
        var onlySprint = sprintRangeCubes.Except (advanceRangeCubes);

        return onlySprint.Contains(cube);
    }
}