using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid3 <TGridObject>
{
    private TGridObject[,,] gridArray;

    public Grid3(int sizeX, int sizeY, int sizeZ)
    {
        gridArray = new TGridObject[sizeX, sizeY, sizeZ];
    }

    public TGridObject Get(int x, int y, int z)
    {
        if (x >= 0 && x < gridArray.GetLength(0) &&
            y >= 0 && y < gridArray.GetLength(1) &&
            z >= 0 && z < gridArray.GetLength(2))
        {
            return gridArray[x, y, z];
        }
        else
        {
            return default(TGridObject);
        }
    }
    public void Add(TGridObject obj, int x, int y, int z)
    {
        gridArray[x, y, z] = obj;
    }
    public void Remove(TGridObject obj)
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    for (int z = 0; z < gridArray.GetLength(2); z++)
                    {
                        if (gridArray[x, y, z].Equals(obj))
                        {
                            gridArray[x, y, z] = default(TGridObject);
                        }
                    }
                }
        }
    }
}
