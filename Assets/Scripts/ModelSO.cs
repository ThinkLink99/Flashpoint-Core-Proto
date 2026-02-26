using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]
public class ModelSO : ScriptableObject
{
    [JsonProperty] public int baseSizeMM = 32;
    [JsonProperty] public string factionName = "UNSC";
    [JsonProperty] public string unitName = "Spartan MK VII";

    // TODO: Add Keyword functionality

    [JsonProperty] public int unitHP = 4;
    [JsonProperty] public int unitArmor = 2;
    [JsonProperty] public int unitAdvanceSpeed = 2;
    [JsonProperty] public int unitSprintSpeed = 3;
    [JsonProperty] public int unitRange = 4;
    [JsonProperty] public int unitFight = 4;
    [JsonProperty] public int unitSave = 4;

    // TODO: Add Weapon loadout functionality
}
