using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ModelInformationController : MonoBehaviour
{
    [SerializeField] private UIDocument _document;

    [SerializeField] public PlayerController playerController;
    [SerializeField] private Model selectedModel;
    [SerializeField] private List<Model> activationsRemaining;

    [SerializeField] VisualElement header = null;
    [SerializeField] VisualElement listView = null;
    [SerializeField] VisualElement modelActions = null;
    [SerializeField] VisualElement modelCard = null;

    public Model SelectedModel => selectedModel;

    private void Awake()
    {
        var root = _document.rootVisualElement;

        header = root.Q("HeaderBar");
        listView = root.Q<ListView>("ActivationsList");
        modelActions = root.Q("ModelActions");
        modelCard = modelActions.Q("ModelCard").Q("Card");

        activationsRemaining = new List<Model>();

        listView.dataSource = activationsRemaining;
    }

    private void Start()
    {
        HideUI();
    }

    public void ControllerChanged()
    {
        activationsRemaining.Clear();
        if (playerController.ActivationsRemaining.Count > 0)
        {
            activationsRemaining.AddRange(playerController.ActivationsRemaining);
        }

        ToggleActivationsList();
    }
    private void ToggleActivationsList (bool visible = true)
    {
        listView.visible = visible;
    }
    public void ShowUI ()
    {
        header.visible = true;
    }
    public void HideUI()
    {
        header.visible = false;
        listView.visible = false;
        modelActions.visible = false;
    }

    public void OnModelSelected (Component sender, object data)
    {
        var model = data as Model;
        if (model != null)
        {
            selectedModel = model;
            modelCard.dataSource = model.ModelConfiguration;

            modelActions.visible = true;
        }
    }
    public void OnModelDeselected (Component sender, object data)
    {
        selectedModel = null;
        modelActions.visible = false;
    }
}