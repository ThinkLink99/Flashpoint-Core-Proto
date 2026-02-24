using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Terrain : MonoBehaviour
{
    [JsonProperty("id")]
    public string id;
    [JsonProperty("world_position")]
    public Vector3S worldPositon;
    [JsonProperty("world_rotation")]
    public Vector3S worldRotation;


    public void SetPositionFromTransform (Transform transform)
    {
        worldPositon = new Vector3S(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }
    public void SetRotationFromTransform(Transform transform)
    {
        worldRotation = new Vector3S(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }
}

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