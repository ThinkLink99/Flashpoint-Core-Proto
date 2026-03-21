using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class StringGameObjectMap : SerializableDictionaryBase<string, GameObject> { }
[Serializable]
public class StringScriptableObjecttMap : SerializableDictionaryBase<string, ScriptableObject> { }
[Serializable]
public class StringModelConfigMap : SerializableDictionaryBase<string, ModelConfiguration> { }