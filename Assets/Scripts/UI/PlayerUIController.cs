using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    private Model selectedModel = null;

    // UI Elements that will need to check against the Models activation status and remaining AP to determine visibility

    // Activate Button
    [SerializeField] private GameObject activateButton;
    // Advance Button (Move)
    [SerializeField] private GameObject advanceButton;
    // Sprint Button
    [SerializeField] private GameObject sprintButton;

    // Shoot Button (should not show if weapon has long keyword or other keyword with similar effect. For now, always show if unit has atleast 1 remaining AP)
    [SerializeField] private GameObject shootButton;


    private bool CanActivate()
    {
        if (selectedModel == null) return false;
        if (selectedModel.ActionController.IsActivated == false)
        {
            return !selectedModel.ActionController.HasActivated; // just wrap the action controller here
        }
        else return false;
    }
    private bool CanAdvance()
    {
        if (selectedModel == null) return false;

        return selectedModel.ActionController.IsActivated && selectedModel.ActionController.RemainingAP > 0;
    }
    private bool CanSprint()
    {
        if (selectedModel == null) return false;

        return selectedModel.ActionController.IsActivated && selectedModel.ActionController.RemainingAP > 1;
    }
    private bool CanShoot()
    {
        if (selectedModel == null) return false;

        return selectedModel.ActionController.IsActivated && selectedModel.ActionController.RemainingAP > 0;
    }

    private void Update()
    {
        DoUI();
    }

    public void DoUI ()
    {
        ToggleActivationButton();
        //ToggleSprintButton();
        ToggleAdvanceButton();
        ToggleShootButton();
    }

    public void ToggleActivationButton()
    {
        if (CanActivate())
        {
            activateButton?.SetActive(true);
        }
        else
        {
            activateButton?.SetActive(false);
        }
    }
    public void ToggleAdvanceButton()
    {
        if (CanAdvance())
        {
            advanceButton?.SetActive(true);
        }
        else
        {
            advanceButton?.SetActive(false);
        }
    }
    public void ToggleSprintButton()
    {
        if (CanSprint())
        {
            sprintButton?.SetActive(true);
        }
        else
        {
            sprintButton?.SetActive(false);
        }
    }
    public void ToggleShootButton()
    {
        if (CanShoot())
        {
            shootButton?.SetActive(true);
        }
        else
        {
            shootButton?.SetActive(false);
        }
    }


    public void OnModelSelected(Component sender, object data)
    {
        if (data is not Model model) return;
        if (selectedModel == model) return;

        selectedModel = model;
    }
    public void OnModelDeselected(Component sender, object data)
    {
        if (data is not Model model) return;
        if (selectedModel != model) return;

        selectedModel = null;
    }
}
