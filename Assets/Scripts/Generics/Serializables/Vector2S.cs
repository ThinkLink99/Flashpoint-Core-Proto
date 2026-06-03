using Newtonsoft.Json;
using System;
using UnityEngine;

public class Vector2S
{
    [JsonProperty("x")]
    public float x;
    [JsonProperty("y")]
    public float y;

    public Vector2S(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
    public override string ToString()
    {
        return $"({x}, {y})";
    }
}

[Serializable, JsonObject]
public class Vector2IntS
{
    [JsonProperty("x"), SerializeField]
    public int x;
    [JsonProperty("y"), SerializeField]
    public int y;

    public Vector2IntS(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Vector2Int ToVector2()
    {
        return new Vector2Int(x, y);
    }
    public override string ToString()
    {
        return $"({x}, {y})";
    }
}