using UnityEngine;

[CreateAssetMenu(menuName = "UI/New Theme", fileName = "New Theme")]
public class ThemeSO : ScriptableObject
{
    [Header("Primary")]
    public Color text_primary;
    public Color background_primary;

    [Header("Secondary")]
    public Color text_secondary;
    public Color background_secondary;

    [Header("Tertiary")]
    public Color text_tertiary;
    public Color background_tertiary;

    [Header("Other")]
    public Color disable;

    public Color GetTextColor (ThemeColor color)
    {
        switch (color)
        {
            case ThemeColor.Primary:
                return text_primary;
            case ThemeColor.Secondary:
                return text_secondary;
            case ThemeColor.Tertiary:
                return text_tertiary;
            default:
                return disable;
        }
    }
    public Color GetBackgroundColor(ThemeColor color)
    {
        switch (color)
        {
            case ThemeColor.Primary:
                return background_primary;
            case ThemeColor.Secondary:
                return background_secondary;
            case ThemeColor.Tertiary:
                return background_tertiary;
            default:
                return disable;
        }
    }
}

public enum ThemeColor
{
    Primary, Secondary, Tertiary
}