using Newtonsoft.Json;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class WeaponConfiguration : ScriptableObject
{
    [JsonProperty, CreateProperty] public string weaponName = "MA40 Assault Rifle";
    [JsonProperty, CreateProperty] public string weaponDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec a diam lectus. Sed sit amet ipsum mauris. Maecenas congue ligula ac quam viverra nec consectetur ante hendrerit. Donec et mollis dolor. Praesent et diam eget libero egestas mattis sit amet vitae augue. Nam tincidunt congue enim, ut porta lorem lacinia consectetur. Donec ut libero sed arcu vehicula ultricies a non tortor. Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    [JsonProperty, CreateProperty] public Sprite weaponImage;
    [JsonProperty , CreateProperty] public int weaponCost = 20;
    [JsonProperty, CreateProperty] public int weaponRange = 4; // 0 For Melee, any number greater than 0 for Ranged
    [JsonProperty, CreateProperty] public int weaponArmorPiercing = 2;

    // keywords and special rules will be added in the future, but for now we will just have the basic stats
    [JsonProperty, CreateProperty] public List<KeywordInstance> keywords;

    [CreateProperty] public string KeywordStringList => string.Join(", ", keywords);
}