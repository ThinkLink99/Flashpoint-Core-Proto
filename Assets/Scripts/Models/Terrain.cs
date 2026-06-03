using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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

    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    {
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    }
}
