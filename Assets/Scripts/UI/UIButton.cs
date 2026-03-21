using TMPro;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] protected GameEvent onButtonClicked;
    [SerializeField] protected TextMeshProUGUI buttonTextUGUI;

    [SerializeField] private string buttonText = "Button";

    private void Awake()
    {
        if (buttonTextUGUI == null) buttonTextUGUI = GetComponentInChildren<TextMeshProUGUI>();
        buttonTextUGUI.text = buttonText;
    }

    public virtual void ShowButton()
    {
        this.gameObject.SetActive(true);
    }
    public virtual void HideButton()
    {
        this.gameObject.SetActive(false);
    }
    public virtual void OnButtonClick()
    {
        onButtonClicked?.Raise(this, null);
    }
}