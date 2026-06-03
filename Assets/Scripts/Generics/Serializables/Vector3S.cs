using Newtonsoft.Json;
using UnityEngine;

public class Vector3S
{
    [JsonProperty("x")]
    public float x;
    [JsonProperty("y")]
    public float y;
    [JsonProperty("z")]
    public float z;
    public Vector3S(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}
