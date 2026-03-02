using UnityEngine;

[CreateAssetMenu(fileName = "UnitSet", menuName = "ScriptableObjects/UnitSetScriptableObject", order = 1)]
public class ModelSetScriptableObject : ScriptableObject
{
    public StringGameObjectMap units;
}
