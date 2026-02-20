using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Map
    {
        [JsonProperty("map")]
        public string MapName { get; set; }
        [JsonProperty("size")]
        public MapSize MapSize { get; set; }
        [JsonProperty("cube_size")]
        public float CubeSize { get; set; } = 76.2f;
        [JsonProperty("layers")]
        public Layer[] Layers { get; set; }

        public static Map Load(TextAsset jsonFile)
        {
            Map mapInJson = JsonConvert.DeserializeObject<Map>(jsonFile.text);
            // do we want to do any of the map building here? or just return the map and have the map builder do it?
            return mapInJson;
        }
        public void Save(string path)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(path, json);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct MapSize
    {
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("z")]
        public int Z { get; set; }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public struct Layer
    {
        [JsonProperty("objects")]
        public CubeInfo[,] Objects { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public struct CubeInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}