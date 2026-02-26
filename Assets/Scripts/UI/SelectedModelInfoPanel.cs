using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectedModelInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modelNameTextBox;

    private Model selectedModel = null;

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);

        if (selectedModel?.ActionController.RemainingAP > 0)
        {

        }
    }
    public void HidePanel()
    {
        this.gameObject.SetActive(false);
    }

    public void OnModelSelected(Component sender, object data)
    {
        if (data is not Model model) return;
        if (selectedModel == model) return;

        selectedModel = model;

        ShowPanel();
        modelNameTextBox?.SetText(model.name);
    }
    public void OnModelDeselected(Component sender, object data)
    {
        if (data is not Model model) return;
        if (selectedModel != model) return;

        selectedModel = null;

        modelNameTextBox?.SetText("");
        HidePanel();
    }
}
