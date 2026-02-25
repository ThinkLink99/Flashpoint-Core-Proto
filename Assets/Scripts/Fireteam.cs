using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class Fireteam
{
    [SerializeField] private List<Unit> models = new List<Unit>();

    [JsonProperty("models")]
    public List<Unit> Models
    {
        get => models; 
        set => models = value ?? new List<Unit>(); // Ensure we never set it to null

    }

    // these are future to implement later but im leaving them here for planning
    // public List<Weapon> Weapons { get; set; } // Any bought weapons
    // public List<Item> Weapons { get; set; } // Any bought items
    // public List<CommandOrder> CommandOrders { get; set; } // Any bought command orders

}