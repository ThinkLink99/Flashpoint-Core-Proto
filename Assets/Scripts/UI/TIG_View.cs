using UnityEngine;
using UnityEngine.UI;
public class TIG_View : TIG_UIBase 
{
    [SerializeField] private ViewSO viewData;

    [SerializeField] private GameObject header;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject footer;

    [SerializeField] private Image headerImage;
    [SerializeField] private Image contentImage;
    [SerializeField] private Image footerImage;

    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

    public override void Setup()
    {
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        headerImage = header.GetComponent<Image>();
        contentImage = content.GetComponent<Image>();
        footerImage = footer.GetComponent<Image>();
    }
    public override void Configure()
    {
        verticalLayoutGroup.padding = viewData.padding;
        verticalLayoutGroup.spacing = viewData.spacing;

        headerImage.color = viewData.theme.background_primary;
        contentImage.color = viewData.theme.background_secondary;
        footerImage.color = viewData.theme.background_tertiary;
    }
}
