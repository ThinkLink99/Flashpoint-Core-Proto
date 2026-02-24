using UnityEngine;

[CreateAssetMenu(fileName = "UnitSet", menuName = "ScriptableObjects/UnitSetScriptableObject", order = 1)]
public class UnitSetScriptableObject : ScriptableObject
{
    public StringGameObjectMap units;
}
