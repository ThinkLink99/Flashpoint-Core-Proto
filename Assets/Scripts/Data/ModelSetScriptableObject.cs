using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSet", menuName = "ScriptableObjects/UnitSetScriptableObject", order = 1)]
public class ModelSetScriptableObject : ScriptableObject
{
    public StringModelConfigMap units;

    public ModelConfiguration[] ToArray()
    {
        return units.Values.ToArray();
    }
}
