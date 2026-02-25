using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class Map
{
    [SerializeField] private string mapName;
    [SerializeField] private Vector3IntS mapSize;
    [SerializeField] private float cubeSize = 76.2f;
    [SerializeField] private List<DeploymentZone> deploymentZones;

    [JsonProperty("map")]
    public string MapName { get => mapName; set => mapName = value; }
    [JsonProperty("size")]
    public Vector3IntS MapSize { get => mapSize; set => mapSize = value; }
    [JsonProperty("cube_size")]
    public float CubeSize { get => cubeSize; set => cubeSize = value; }

    [JsonProperty("deployment_zones")]
    public List<DeploymentZone> DeploymentZones { get => deploymentZones; set => deploymentZones = value; }

    [JsonProperty("terrain")]
    public Terrain[] Terrain { get; set; }

    public Grid3<Cube> MapGrid { get; set; }

    public static Map Load(TextAsset jsonFile)
    {
        Map mapInJson = JsonConvert.DeserializeObject<Map>(jsonFile.text);
        return mapInJson;
    }

    public void AddTerrain (Terrain terrain)
    {
        // add the terrain to the map's terrain array
        var terrainList = new List<Terrain>(Terrain);
        terrainList.Add(terrain);
        Terrain = terrainList.ToArray();
    }
    public void AddTerrain(IEnumerable<Terrain> terrain)
    {
        // add the terrain to the map's terrain array
        var terrainList = new List<Terrain>(Terrain);
        terrainList.AddRange(terrain);
        Terrain = terrainList.ToArray();
    }

    public void Save(string path)
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);
    }
}

public enum TeamId     {
    Red = 1,
    Blue = 2
}

[Serializable, JsonObject(MemberSerialization.OptIn)]
public struct DeploymentZone
{
    [JsonProperty("teamId"), SerializeField]
    public TeamId teamId;
    [JsonProperty("squares"), SerializeField]
    public List<Vector2IntS> squares;
}
