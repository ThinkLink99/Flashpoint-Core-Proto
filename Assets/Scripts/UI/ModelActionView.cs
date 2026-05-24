using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class ModelActionView : MonoBehaviour
{
    private ModelActionViewModel viewModel;

    [SerializeField] private UIDocument _document;

    [Header("References")]
    [SerializeField] public PlayerController playerController;
    [SerializeField] private Model selectedModel;
    [SerializeField] private ModelActionController actionController;
    [SerializeField] private List<Model> activationsRemaining;

    [Header("Templates")]
    [SerializeField] private VisualTreeAsset weaponCardTemplate;

    [Header("Events")]
    [SerializeField] private GameEvent onModelActivated;
    [SerializeField] private GameEvent onModelDeactivated;
    [SerializeField] private GameEvent onModelMoveActivated;
    [SerializeField] private GameEvent onModelMoveDeactivated;
    [SerializeField] private GameEvent onModelMoveConfirmed;
    [SerializeField] private GameEvent onModelShootActivated;
    [SerializeField] private GameEvent onModelShootDeactivated;
    [SerializeField] private GameEvent onModelShootConfirmed;

    VisualElement header;
    VisualElement listView;
    VisualElement modelCard;
    VisualElement modelWeapons;
    VisualElement modelActions;

    Label lblCurrentHP;

    Button btnActivateModel;
    Button btnDeactivateModel;
    Image imgActivated;

    private bool moving;
    Button ModelMoveButton;
    Button ConfirmMoveButton;

    private bool shooting;
    Button ModelShootButton;
    Button ConfirmShootButton;

    VisualElement debugPanel;
    Label lblUnitName;
    Label lblUnitPos;
    Label lblAPRemaining;
    Label lblUnitIsActivated;
    Label lblUnitHasActivated;
    Label lblUnitMoved;
    Label lblUnitShot;
    Label lblUnitCrouched;

    public Model SelectedModel => selectedModel;

    private void Awake()
    {
        var root = _document.rootVisualElement;
        header = root.Q("HeaderBar");
        listView = root.Q<ListView>("ActivationsList");
        modelCard = root.Query("ModelCard").First().Query("Card").First();
        modelActions = root.Query("ModelActions");
        modelWeapons = root.Query("Weapons").First().Q("unity-content-container");

        lblCurrentHP = modelCard.Q<Label>("lblCurrentHP");

        SetupActivationButtons(root);
        SetupMoveButton(root);
        SetupConfirmMoveButton(root);
        SetupShootButton(root);
        SetupConfirmShootButton(root);

        debugPanel = root.Q("Debug");
        lblUnitName = debugPanel.Q<Label>("lblUnitName");
        lblUnitPos = debugPanel.Q<Label>("lblUnitPos");
        lblAPRemaining = debugPanel.Q<Label>("lblActionPointsRemain");
        lblUnitIsActivated = debugPanel.Q<Label>("lblUnitIsActivated");
        lblUnitHasActivated = debugPanel.Q<Label>("lblUnitHasActivated");
        lblUnitMoved = debugPanel.Q<Label>("lblUnitMoved");
        lblUnitShot = debugPanel.Q<Label>("lblUnitShot");
        lblUnitCrouched = debugPanel.Q<Label>("lblUnitCrouched");
    }
    private void Update()
    {
        // keep viewModel in sync; it will raise StateChanged only when values actually change
        viewModel?.Refresh();
    }
    private void OnDestroy()
    {
        DetachViewModel();
        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnWorldModelSelected -= OnWorldModelSelected;
            gm.OnWorldDestinationSelected -= OnWorldDestinationSelected;
            gm.OnWorldTargetSelected -= OnWorldTargetSelected;
        }
    }

    private void OnWorldModelSelected(object sender, ModelSelectedEventArgs e)
    {
        // Reconcile selection UI: only update selection if the world event concerns this client
        // or if we want all clients to show the selected model. For now, update UI to reflect
        // authoritative selection unconditionally.
        OnModelSelected(null, e.Model);
    }
    private void OnWorldDestinationSelected(object sender, DestinationSelectedEventArgs e)
    {
        // Update view to reflect authoritative destination selection if relevant
        // (e.g., show ghost placement or update model position after move completes)
        // For now, just update the internal context if the selected model belongs to the requester
        // or matches the current view's selected model.
    }
    private void OnWorldTargetSelected(object sender, TargetSelectedEventArgs e)
    {
        // Update target UI similarly
    }

    // Configure the view with a new player controller. This is safer than assigning the
    // field externally because it lets the view clear internal state and rebuild UI.
    public void SetPlayerController(PlayerController controller)
    {
        // Detach any existing view model and clear selection
        DetachViewModel();

        playerController = controller;
        selectedModel = null;
        actionController = null;
        ClearWeaponList();

        // Rebuild activations list and update UI
        ControllerChanged();
        UpdateUI();
    }

    // Update the activations list when the assigned controller changes
    public void ControllerChanged()
    {
        if (activationsRemaining == null) activationsRemaining = new List<Model>();
        activationsRemaining.Clear();
        if (playerController != null && playerController.ActivationsRemaining != null)
        {
            activationsRemaining.AddRange(playerController.ActivationsRemaining);
        }

        ToggleActivationsList();
    }

    // Show or hide the activations list. Implementation is intentionally minimal now;
    // later this can populate a ListView control or perform more complex UI updates.
    private void ToggleActivationsList(bool visible = true)
    {
        // no-op placeholder: the activations list UI is not yet implemented
    }

    // Simple public API for show/hide used by the state machine
    public void ShowUI()
    {
        if (header != null) header.visible = true;
        UpdateUI();
    }
    public void HideUI()
    {
        if (header != null) header.visible = false;
        if (modelCard != null) modelCard.visible = false;
        if (modelActions != null) modelActions.visible = false;
        UpdateUI();
    }
    private void UpdateUI()
    {
        // show/hide model sections
        modelCard.visible = selectedModel != null;
        modelActions.visible = selectedModel != null;

        // activation buttons
        if (viewModel == null)
        {
            btnActivateModel.style.display = DisplayStyle.None;
            btnDeactivateModel.style.display = DisplayStyle.None;
            imgActivated.style.display = DisplayStyle.None;
        }
        else if (viewModel.IsActivated && playerController.IsLocalPlayer && selectedModel.playerControlling == playerController)
        {

            imgActivated.style.display = DisplayStyle.None;
            btnActivateModel.style.display = DisplayStyle.None;
            btnDeactivateModel.style.display = DisplayStyle.Flex;
        }
        else if (playerController.IsLocalPlayer && selectedModel.playerControlling == playerController)
        {
            imgActivated.style.display = DisplayStyle.None;
            btnActivateModel.style.display = DisplayStyle.Flex;
            btnDeactivateModel.style.display = DisplayStyle.None;
        }

        // move / shoot buttons
        ModelMoveButton.style.display = GetMoveButtonDisplay();

        ConfirmMoveButton.style.display = moving ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateHealth();
        UpdateShields();
        UpdateSelectedWeapon();
        //UpdateDebugInfo();
    }
    private void UpdateSelectedWeapon()
    {
        for (int i = 0; i < modelWeapons.childCount; i++)
        {
            modelWeapons[i].RemoveFromClassList("equipment-card-selected");

            var weapon = modelWeapons[i].dataSource as Weapon;
            if (weapon != null && weapon == viewModel.SelectedWeapon)
                modelWeapons[i].AddToClassList("equipment-card-selected");
        }
    }
    private void UpdateHealth()
    {
        if (viewModel == null) return;

        lblCurrentHP.text = selectedModel.CurrentHealth.ToString();
    }
    private void UpdateShields ()
    {
        if (viewModel == null) return;

        modelCard.Q<Image>("Shield1").style.visibility = Visibility.Hidden;
        modelCard.Q<Image>("Shield2").style.visibility = Visibility.Hidden;
        modelCard.Q<Image>("Shield3").style.visibility = Visibility.Hidden;
        modelCard.Q<Image>("Shield4").style.visibility = Visibility.Hidden;


        // query model card for shields 1-4 depending on count in view model
        for (int i = 0; i < viewModel.ShieldCount; i++)
        {
            var shield = modelCard.Q<Image>("Shield" + (i + 1));
            shield.style.visibility = Visibility.Visible;
            if (i < viewModel.ShieldUses)
            {
                shield.RemoveFromClassList("model-card-shield-inactive");
                shield.AddToClassList("model-card-shield-active");
            }
            else
            {
                shield.RemoveFromClassList("model-card-shield-active");
                shield.AddToClassList("model-card-shield-inactive");
            }
        }
    }
    private void UpdateDebugInfo()
    {
        if (selectedModel == null)
        {
            lblUnitName.text = "None";
            lblUnitPos.text = "N/A";
            lblAPRemaining.text = "N/A";
            lblUnitIsActivated.text = "False";
            lblUnitHasActivated.text = "False";
            lblUnitMoved.text = "False";
            lblUnitShot.text = "False";
            return;
        }

        lblUnitName.text = $"{selectedModel.ModelConfiguration.unitName}";
        lblUnitPos.text = $"{selectedModel.transform.localPosition}";
        lblAPRemaining.text = viewModel != null ? viewModel.RemainingAP.ToString() : "N/A";
        lblUnitIsActivated.text = (viewModel?.IsActivated ?? false).ToString();
        lblUnitHasActivated.text = (viewModel?.HasActivated ?? false).ToString();
        lblUnitMoved.text = (viewModel?.HasMoved ?? false).ToString();
        lblUnitShot.text = (viewModel?.HasShot ?? false).ToString();
        lblUnitCrouched.text = "N/A";
    }

    private void SetupActivationButtons(VisualElement root)
    {
        btnActivateModel = root.Q<Button>("btnActivateModel");
        btnDeactivateModel = root.Q<Button>("btnDeactivateModel");
        imgActivated = root.Q<Image>("imgActivated");

        btnActivateModel.RegisterCallback<ClickEvent>((evt) =>
        {
            // local feedback
            viewModel?.RequestActivate();
            // authoritative handling
            onModelActivated?.Raise(this, selectedModel);
        });

        btnDeactivateModel.RegisterCallback<ClickEvent>((evt) =>
        {
            viewModel?.RequestDeactivate();
            onModelDeactivated?.Raise(this, selectedModel);
        });
    }

    private void SetupMoveButton(VisualElement root)
    {
        ModelMoveButton = root.Q<Button>("btnMoveModel");
        ModelMoveButton.RegisterCallback<ClickEvent>((evt) =>
        {
            moving = !moving;
            if (moving)
                onModelMoveActivated?.Raise(this, selectedModel);
            else
                onModelMoveDeactivated?.Raise(this, selectedModel);

            UpdateUI();
        });
    }
    private DisplayStyle GetMoveButtonDisplay()
    {
        if (viewModel == null) return DisplayStyle.None;
        if (viewModel.HasMoved) return DisplayStyle.None;
        if (viewModel.RemainingAP < 1) return DisplayStyle.None;
        if (viewModel.HasActivated || !viewModel.IsActivated) return DisplayStyle.None;
        return DisplayStyle.Flex;
    }
    private void SetupConfirmMoveButton(VisualElement root)
    {
        ConfirmMoveButton = root.Query<Button>("btnConfirmMove");
        ConfirmMoveButton.style.display = DisplayStyle.None;
        ConfirmMoveButton.RegisterCallback<ClickEvent>((evt) =>
        {
            moving = false;
            onModelMoveConfirmed?.Raise(this, selectedModel);
            UpdateUI();
        });
    }

    private void SetupShootButton(VisualElement root)
    {

    }
    private void SetupConfirmShootButton(VisualElement root)
    {
        ConfirmShootButton = root.Query<Button>("btnConfirmShoot");
        ConfirmShootButton.style.display = DisplayStyle.None;
        ConfirmShootButton.RegisterCallback<ClickEvent>((evt) =>
        {
            onModelShootConfirmed?.Raise(this, selectedModel);
            shooting = false;
            UpdateUI();
        });
    }

    private TemplateContainer BuildWeaponCard(Weapon weapon)
    {
        var card = weaponCardTemplate.CloneTree();
        card.AddToClassList ("equipment-card");
        card.dataSource = weapon;

        if (viewModel.SelectedWeapon == weapon) card.AddToClassList("equipment-card-selected");

        card.Q<VisualElement>("card-body").AddToClassList("weapon-card-small");
        card.Q<Label>("lblWeaponName").text = weapon.WeaponConfiguration.weaponName;
        card.Q<Image>("imgWeaponImage").sprite = weapon.WeaponConfiguration.weaponImage;
        card.Q<Label>("lblRange").text = weapon.WeaponConfiguration.weaponRange.ToString();
        card.Q<Label>("lblAP").text = weapon.WeaponConfiguration.weaponArmorPiercing.ToString();

        card.RegisterCallback<ClickEvent>((evt) =>
        {
            // TODO: this is a bit hacky; we should ideally have the view model track the selected weapon and update the card styles accordingly
            // in UpdateUI, rather than relying on click events to manage UI state.
            // This could lead to bugs if the view model changes the selected weapon for any reason other than a direct click (e.g., deselecting the current weapon when it's no longer valid).
            // For now, this is a simple way to provide visual feedback on selection, but it may need to be refactored for robustness as the UI becomes more complex.

            // Add the functionality of starting shoot mode and target selection when a weapon is clicked, if the weapon is not already selected. If the weapon is already selected, clicking it again should deselect it and exit shoot mode.
             if (viewModel.SelectedWeapon == weapon)
            {
                // Deselect the weapon and exit shoot mode
                viewModel.RequestSelectWeapon(null);
                shooting = false;
                onModelShootDeactivated?.Raise(this, selectedModel);
            }
            else
            {
                // Select the new weapon and enter shoot mode
                viewModel.RequestSelectWeapon(weapon);
                shooting = true;
                onModelShootActivated?.Raise(this, selectedModel);
            }
        });
        return card;
    }

    private void BuildWeaponList()
    {
        modelWeapons.Clear();
        if (selectedModel == null) return;
        var weapons = selectedModel.GetComponentsInChildren<Weapon>(); // Get all weapons on the model. this allows us to have only runtime weapons if the player drops or picks up new ones

        foreach (var weapon in weapons)
            modelWeapons.Add(BuildWeaponCard(weapon));
    }
    private void ClearWeaponList()
    {
        modelWeapons.Clear();
    }

    private void DetachViewModel()
    {
        if (viewModel != null)
            viewModel.StateChanged -= UpdateUI;
        viewModel = null;
    }

    // called by external event system when selection changes
    public void OnModelSelected(Component sender, object data)
    {
        var model = data as Model;
        if (model == null) return;

        DetachViewModel();

        selectedModel = model;
        actionController = model.ActionController;

        viewModel = new ModelActionViewModel(model);
        viewModel.StateChanged += UpdateUI;

        modelCard.dataSource = model.ModelConfiguration;
        BuildWeaponList();

        UpdateUI();
    }
    public void OnModelDeselected(Component sender, object data)
    {
        DetachViewModel();
        selectedModel = null;
        actionController = null;
        ClearWeaponList();
        UpdateUI();
    }
}