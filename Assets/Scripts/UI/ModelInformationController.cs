using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ModelInformationController : MonoBehaviour
{
    [SerializeField] private UIDocument _document;

    [Header("References")]
    [SerializeField] public PlayerController playerController;
    [SerializeField] private Model selectedModel;
    [SerializeField] private ModelActionController actionController;
    [SerializeField] private List<Model> activationsRemaining;

    [Header("Events")]
    [SerializeField] private GameEvent onModelActivated;
    [SerializeField] private GameEvent onModelDeactivated;
    [SerializeField] private GameEvent onModelMoveActivated;
    [SerializeField] private GameEvent onModelMoveDeactivated;
    [SerializeField] private GameEvent onModelMoveConfirmed;
    [SerializeField] private GameEvent onModelShootActivated;
    [SerializeField] private GameEvent onModelShootDeactivated;
    [SerializeField] private GameEvent onModelShootConfirmed;

    VisualElement header = null;
    VisualElement listView = null;
    VisualElement modelCard = null;

    VisualElement modelActions = null;

    Button btnActivateModel = null;
    Button btnDeactivateModel = null;
    Image imgActivated = null;

    private bool moving = false;
    Sprite moveIcon;
    Sprite cancelMoveIcon;
    Button ModelMoveButton = null;
    Button ConfirmMoveButton = null;

    private bool shooting = false;
    Sprite shootIcon;
    Sprite cancelShootIcon;
    Button ModelShootButton = null;
    Button ConfirmShootButton = null;

    VisualElement debugPanel = null;
    Label lblUnitName = null;
    Label lblUnitPos = null;
    Label lblAPRemaining = null;
    Label lblUnitIsActivated = null;
    Label lblUnitHasActivated = null;
    Label lblUnitMoved = null;
    Label lblUnitShot = null;
    Label lblUnitCrouched = null;

    public Model SelectedModel => selectedModel;

    public bool IsActivated => actionController != null ? actionController.IsActivated : false;
    public bool HasActivated => actionController != null ? actionController.HasActivated : false;
    public bool HasMoved => actionController != null ? actionController.HasMoved : false;
    public bool HasShot => actionController != null ? actionController.HasShot : false;

    private void Awake()
    {
        var root = _document.rootVisualElement;
        header = root.Q("HeaderBar");
        listView = root.Q<ListView>("ActivationsList");
        modelCard = root.Query("ModelCard").First().Query("Card").First();
        modelActions = root.Query("ModelActions");

        SetupActivationButtons(root);

        SetupMoveButton(root);
        SetupConfirmMoveButton(root);

        SetupShootButton(root);
        SetupConfirmShootButton(root);

        debugPanel = root.Q("Debug");
        lblUnitName = debugPanel.Q<Label>("lblUnitName");
        lblUnitPos = debugPanel.Q<Label>("lblUnitPos");
        lblAPRemaining = debugPanel.Q<Label>("lblAPRemaining");
        lblUnitIsActivated = debugPanel.Q<Label>("lblUnitIsActivated");
        lblUnitHasActivated = debugPanel.Q<Label>("lblUnitHasActivated");
        lblUnitMoved = debugPanel.Q<Label>("lblUnitMoved");
        lblUnitShot = debugPanel.Q<Label>("lblUnitShot");
        lblUnitCrouched = debugPanel.Q<Label>("lblUnitCrouched");
    }
    private void Start() { }
    private void Update()
    {
        ModelMoveButton.style.display = GetMoveButtonDisplay();
        ModelShootButton.style.display = GetShootButtonDisplay();

        if (moving) ConfirmMoveButton.style.display = DisplayStyle.Flex;
        else ConfirmMoveButton.style.display = DisplayStyle.None;

        if (shooting) ConfirmShootButton.style.display = DisplayStyle.Flex;
        else ConfirmShootButton.style.display = DisplayStyle.None;

        if (IsActivated)
        {
            imgActivated.style.display = DisplayStyle.None;
            btnActivateModel.style.display = DisplayStyle.None;
            btnDeactivateModel.style.display = DisplayStyle.Flex;
        }
        else if (!IsActivated)
        {
            imgActivated.style.display = DisplayStyle.None;
            btnActivateModel.style.display = DisplayStyle.Flex;
            btnDeactivateModel.style.display = DisplayStyle.None;
        }
        else
        {
            imgActivated.style.display = DisplayStyle.Flex;
            btnDeactivateModel.style.display = DisplayStyle.None;
            btnActivateModel.style.display = DisplayStyle.None;
        }

        UpdateDebugInfo();
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
        modelCard.visible = false;
        modelActions.visible = false;
    }

    private void UpdateDebugInfo()
    {
        if (selectedModel == null) return;

        Debug.Log($"Updating debug info for {selectedModel?.ModelConfiguration.unitName ?? "None"}");

        lblUnitName.text = $"{selectedModel?.ModelConfiguration.unitName ?? "None"}";
        lblUnitPos.text = $"{selectedModel?.transform.localPosition.ToString() ?? "None"}";
        //lblAPRemaining.text = $"{actionController?.RemainingAP.ToString() ?? "N/A"}";
        lblUnitIsActivated.text = IsActivated.ToString();
        lblUnitHasActivated.text = HasActivated.ToString();
        lblUnitMoved.text = HasMoved.ToString();
        lblUnitShot.text = HasShot.ToString();
        lblUnitCrouched.text = "N/A";
    }

    private void SetupActivationButtons(VisualElement root)
    {
        btnActivateModel = root.Q<Button>("btnActivateModel");
        btnDeactivateModel = root.Q<Button>("btnDeactivateModel");
        imgActivated = root.Q<Image>("imgActivated");
        btnActivateModel.style.display = DisplayStyle.None;
        btnDeactivateModel.style.display = DisplayStyle.None;
        imgActivated.style.display = DisplayStyle.None;

        btnActivateModel.RegisterCallback<ClickEvent>((evt) =>
        {
            onModelActivated?.Raise(this, selectedModel);
        });
        btnDeactivateModel.RegisterCallback<ClickEvent>((evt) =>
        {
            onModelDeactivated?.Raise(this, selectedModel);
        });
    }

    private void SetupMoveButton(VisualElement root)
    {
        ModelMoveButton = root.Q<Button>("btnMoveModel");
        ModelMoveButton.RegisterCallback<ClickEvent>((evt) =>
        {
            if (moving)
            {
                moving = false;
                ModelShootButton.style.display = DisplayStyle.Flex;
                //ModelMoveButton.iconImage = Background.FromSprite(moveIcon);
                onModelMoveDeactivated?.Raise(this, selectedModel);
            }
            else
            {
                moving = true;
                ModelShootButton.style.display = DisplayStyle.None;
                //ModelMoveButton.iconImage = Background.FromSprite(cancelMoveIcon);
                onModelMoveActivated?.Raise(this, selectedModel);
            }
        });
    }
    private void SetupConfirmMoveButton(VisualElement root)
    {
        ConfirmMoveButton = root.Query<Button>("btnConfirmMove");
        ConfirmMoveButton.style.display = DisplayStyle.None;
        ConfirmMoveButton.RegisterCallback<ClickEvent>((evt) =>
        {
            moving = false;
            //ModelMoveButton.iconImage = Background.FromSprite(moveIcon);
            ModelShootButton.style.display = DisplayStyle.Flex;
            onModelMoveConfirmed?.Raise(this, selectedModel);
        });
    }
    private DisplayStyle GetMoveButtonDisplay()
    {
        if (selectedModel == null) return DisplayStyle.None;
        if (selectedModel.ActionController.HasMoved) return DisplayStyle.None;
        if (selectedModel.ActionController.RemainingAP < 1) return DisplayStyle.None;
        if (HasActivated || !IsActivated) return DisplayStyle.None;

        return DisplayStyle.Flex;
    }

    private void SetupShootButton(VisualElement root)
    {
        ModelShootButton = modelActions.Q<Button>("btnShootModel");
        ModelShootButton.RegisterCallback<ClickEvent>((evt) =>
        {
            if (shooting)
            {
                shooting = false;
                ModelMoveButton.style.display = DisplayStyle.Flex;
                //ModelShootButton.iconImage = Background.FromSprite(shootIcon);
                onModelShootDeactivated?.Raise(this, selectedModel);
            }
            else
            {
                shooting = true;
                ModelMoveButton.style.display = DisplayStyle.None;
                //ModelShootButton.iconImage = Background.FromSprite(cancelShootIcon);
                onModelShootActivated?.Raise(this, selectedModel);
            }
        });
    }
    private void SetupConfirmShootButton(VisualElement root)
    {
        ConfirmShootButton = root.Query<Button>("btnConfirmShoot");
        ConfirmShootButton.style.display = DisplayStyle.None;
        ConfirmShootButton.RegisterCallback<ClickEvent>((evt) =>
        {
            onModelShootConfirmed?.Raise(this, selectedModel);
            shooting = false;
            //ModelShootButton.iconImage = Background.FromSprite(shootIcon);
            ModelMoveButton.style.display = DisplayStyle.Flex;
        });
    }
    private DisplayStyle GetShootButtonDisplay()
    {
        if (selectedModel == null) return DisplayStyle.None;
        if (selectedModel.ActionController.HasShot) return DisplayStyle.None;
        if (selectedModel.ActionController.RemainingAP < 1) return DisplayStyle.None;
        if (HasActivated || !IsActivated) return DisplayStyle.None;

        return DisplayStyle.Flex;
    }

    public void OnModelSelected (Component sender, object data)
    {
        var model = data as Model;
        if (model != null)
        {
            selectedModel = model;
            actionController = model.ActionController;

            modelCard.dataSource = model.ModelConfiguration;
            modelCard.visible = true;
            modelActions.visible = true;
        }
    }
    public void OnModelDeselected(Component sender, object data)
    {
        selectedModel = null;
        actionController = null;
    }
}