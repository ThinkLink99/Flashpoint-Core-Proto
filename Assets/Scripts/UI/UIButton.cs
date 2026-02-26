using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] protected GameEvent onButtonClicked;

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
        onButtonClicked.Raise(this, null);
        HideButton();
    }
}