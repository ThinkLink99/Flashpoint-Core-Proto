using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[JsonObject(MemberSerialization.OptIn)]
[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]
public class ModelConfiguration : ScriptableObject
{
    public GameObject Model;

    [JsonProperty, CreateProperty] public int baseSizeMM = 32;
    [JsonProperty, CreateProperty] public string factionName = "UNSC";
    [JsonProperty, CreateProperty] public string unitName = "Spartan MK VII";
    [JsonProperty, CreateProperty] public string unitDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec a diam lectus. Sed sit amet ipsum mauris. Maecenas congue ligula ac quam viverra nec consectetur ante hendrerit. Donec et mollis dolor. Praesent et diam eget libero egestas mattis sit amet vitae augue. Nam tincidunt congue enim, ut porta lorem lacinia consectetur. Donec ut libero sed arcu vehicula ultricies a non tortor. Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    [JsonProperty, CreateProperty] public Sprite unitImage;

    // TODO: Add Keyword functionality
    [JsonProperty, CreateProperty] public List<KeywordInstance> keywords = new List<KeywordInstance>();

    [JsonProperty, CreateProperty] public int unitCost = 40;
    [JsonProperty, CreateProperty] public int unitHP = 4;
    [JsonProperty, CreateProperty] public int unitArmor = 2;
    [JsonProperty, CreateProperty] public int unitAdvanceSpeed = 2;
    [JsonProperty, CreateProperty] public int unitSprintSpeed = 3;
    [JsonProperty, CreateProperty] public int unitRange = 4;
    [JsonProperty, CreateProperty] public int unitFight = 4;
    [JsonProperty, CreateProperty] public int unitSave = 4;

    // TODO: Add Weapon loadout functionality
    [JsonProperty, CreateProperty] public List<WeaponConfiguration> unitWeapons = new List<WeaponConfiguration>();
}