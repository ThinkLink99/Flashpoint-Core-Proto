using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] private GameEvent onButtonClicked;

    public void ShowButton()
    {
        this.gameObject.SetActive(true);
    }
    public void HideButton()
    {
        this.gameObject.SetActive(false);
    }
    public void OnButtonClick()
    {
        onButtonClicked.Raise(this, null);
        HideButton();
    }
}
