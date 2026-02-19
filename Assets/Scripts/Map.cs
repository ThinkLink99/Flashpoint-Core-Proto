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
        [JsonProperty("layers")]
        public Layer[] Layers { get; set; }
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