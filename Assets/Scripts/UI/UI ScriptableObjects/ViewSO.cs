using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/New View", fileName = "New View")]
public class ViewSO : ScriptableObject
{
    public ThemeSO theme;

    public RectOffset padding;
    public float spacing;
}
