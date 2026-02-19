using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainSet", menuName = "ScriptableObjects/TerrainSetScriptableObject", order = 1)]
public class TerrainSetScriptableObject : ScriptableObject
{
    public StringGameObjectMap terrainPieces;
}
[Serializable]
public class StringGameObjectMap : SerializableDictionaryBase<string, GameObject> { }