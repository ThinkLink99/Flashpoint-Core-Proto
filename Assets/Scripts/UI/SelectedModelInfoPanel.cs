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
    }
    public void HidePanel()
    {
        this.gameObject.SetActive(false);
    }

    public void OnSelectedModelChanged (Component sender, object data)
    {
        if (data is Model model)
        {
            selectedModel = model;
            // Update the UI elements to show the new model's information
            Debug.Log("Selected model changed: " + model.name);

            ShowPanel();
            modelNameTextBox?.SetText(model.name);
        }
        else
        {
            selectedModel = null;
            // Clear the UI elements since no model is selected
            Debug.Log("No model selected.");
        }
    }
}
