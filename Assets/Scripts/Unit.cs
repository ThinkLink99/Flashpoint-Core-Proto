using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]
public class Unit : ScriptableObject
{
    public int baseSizeMM = 28;
    public string factionName = "UNSC";
    public string unitName = "Spartan MK VII";

    // TODO: Add Keyword functionality

    public int unitHP = 4;
    public int unitArmor = 2;
    public int unitAdvanceSpeed = 2;
    public int unitSprintSpeed = 3;
    public int unitRange = 4;
    public int unitFight = 4;
    public int unitSave = 4;

    // TODO: Add Weapon loadout functionality
}
