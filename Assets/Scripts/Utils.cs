using UnityEngine;

public class Utils
{
    public static TextMesh CreateWorldText (string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color color = default(Color), TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignment textAlignment = TextAlignment.Center, int sortingOrder = 5000)
    {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, color, textAnchor, textAlignment, sortingOrder);
    }
    public static TextMesh CreateWorldText (Transform parent, string text, Vector3 localPositionn, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject ("world_text", typeof (TextMesh));
        Transform transform = gameObject.transform;

        transform.SetParent (parent);
        transform.localPosition = localPositionn;
        //transform.localScale = Vector3.one;

        TextMesh textMesh = gameObject.GetComponent<TextMesh> ();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

        textMesh.text = text;
        return textMesh;
    }
}
