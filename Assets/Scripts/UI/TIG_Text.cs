using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TIG_Text : TIG_UIBase
{
    [SerializeField] private TextSO textData;
    [SerializeField] private TextMeshProUGUI text;

    public override void Configure()
    {
        text.font = textData.font;
        text.fontSize = textData.size;
        text.alignment = textData.alignment;

        text.color = textData.theme.GetTextColor (textData.color);
    }
    public override void Setup()
    {
        if (text is null) text = GetComponentInChildren<TextMeshProUGUI>();
    }
}
