using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public class Vector3IntS
{
    [JsonProperty("x"), SerializeField]
    public int x;
    [JsonProperty("y"), SerializeField]
    public int y;
    [JsonProperty("z"), SerializeField]
    public int z;
    public Vector3IntS(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }
    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}
