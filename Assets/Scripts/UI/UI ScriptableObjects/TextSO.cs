using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/New Text", fileName = "New Text")]
public class TextSO : ScriptableObject
{
    public ThemeSO theme;

    public ThemeColor color;
    public TMP_FontAsset font;
    public float size;

    public TextAlignmentOptions alignment;
}