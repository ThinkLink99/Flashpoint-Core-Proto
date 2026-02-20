using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tabletop : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Events")]
    [SerializeField] private GameEvent onGameStart;

    [Header ("Model Management")]
    [SerializeField] private bool movingModel = false;

    // Drag state
    [Header("Drag State")]
    [SerializeField] private Model selectedModel;
    [SerializeField] private Rigidbody selectedRb;
    [SerializeField] private Plane dragPlane;
    [SerializeField] private Vector3 dragOffset;
    [SerializeField] private float pickUpHeightFromCube = 1f; // default height above cube when placed


    // Point-Click Movement
    [Header("Point-Click Movement")]
    [SerializeField] private Vector3 selectedPoint = Vector3.zero;
    // Ghost preview instance
    private GameObject ghostInstance;

    // Start is called before the first frame update
    void Start()
    {
        onGameStart.Raise(this, null);
    }
    // Update is called once per frame
    void Update()
    {
        // Handle mouse interactions for click & drag pieces
        var mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        //DoModelDragDrop(ray);
        if (movingModel) DoModelPointAndClickMove(mousePos, ray);
    }

    private void DoModelPointAndClickMove(Vector3 mousePos, Ray ray)
    {
        if (selectedModel == null && Input.GetMouseButtonDown(0))
        {
            Debug.Log("Attempting to select model at mouse position: " + mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                // Try to get the Model component on the hit object (or its parents)
                Model hitModel = null;
                var go = hitInfo.collider.gameObject;
                Debug.Log("Hit object: " + go.name);
                if (!go.TryGetComponent(out hitModel))
                {
                    // check parent chain in case collider is child
                    hitModel = go.GetComponentInParent<Model>();
                    Debug.Log("Hit model in parent: " + (hitModel != null ? hitModel.name : "none"));

                    selectedModel = hitModel;
                }
                else
                {
                    selectedModel = hitModel;
                }
            }
            return; // only attempt to select on initial click, don't also select and set point in same frame
        }

        if (selectedModel != null && Input.GetMouseButtonDown(0))
        {
            // select the world point
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 point = hitInfo.point;
                selectedPoint = point;
            }
        }

        if (selectedModel != null && Input.GetAxis("Cancel") == 1)
        {
            // cancel movement
            selectedPoint = Vector3.zero;
            selectedModel = null;

            // ensure ghost removed if selection cancelled
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
        }

        // Update or create ghost preview based on current selected point/selection
        ShowModelAsGhost();
    }
    private void ShowModelAsGhost()
    {
        // Show a ghost of the model at the selected point, maybe with a transparent material or outline shader, to indicate where it will be placed if the player clicks there.
        // This can help with visualizing the move before committing to it.

        // If there is no selection or no valid target point, remove any existing ghost and return
        if (selectedModel == null || selectedPoint == Vector3.zero)
        {
            Debug.Log("No model selected or no target point, removing ghost if it exists.");
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            return;
        }

        // Create the ghost if it doesn't exist
        if (ghostInstance == null)
        {
            // Instantiate a clone of the selected model's root GameObject
            ghostInstance = Instantiate(selectedModel.gameObject);
            ghostInstance.name = selectedModel.gameObject.name + "_Ghost";

            // Remove or disable interactive components on the ghost
            var modelComp = ghostInstance.GetComponent<Model>();
            if (modelComp != null) Destroy(modelComp);

            // Disable colliders so it doesn't block raycasts/physics
            foreach (var col in ghostInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Remove rigidbodies from the ghost
            foreach (var rb in ghostInstance.GetComponentsInChildren<Rigidbody>())
            {
                Destroy(rb);
            }

            // Put ghost on Ignore Raycast layer if it exists
            int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreLayer != -1)
            {
                foreach (Transform t in ghostInstance.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = ignoreLayer;
                }
            }

            // Make materials semi-transparent
            foreach (var rend in ghostInstance.GetComponentsInChildren<Renderer>())
            {
                var mats = rend.sharedMaterials;
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    Material baseMat = mats[i] != null ? new Material(mats[i]) : new Material(Shader.Find("Standard"));

                    if (baseMat.HasProperty("_Color"))
                    {
                        Color c = baseMat.color;
                        c.a = 0.5f;
                        baseMat.color = c;
                    }

                    // Try to set standard shader to transparent mode
                    baseMat.SetFloat("_Mode", 3f);
                    baseMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    baseMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    baseMat.SetInt("_ZWrite", 0);
                    baseMat.DisableKeyword("_ALPHATEST_ON");
                    baseMat.EnableKeyword("_ALPHABLEND_ON");
                    baseMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    baseMat.renderQueue = 3000;

                    newMats[i] = baseMat;
                }
                rend.materials = newMats;
            }
        }

        // Position and orient the ghost at the selectedPoint, preserving the vertical offset used when picking up pieces
        Vector3 ghostPos = new Vector3(selectedPoint.x, selectedPoint.y + pickUpHeightFromCube, selectedPoint.z);
        ghostInstance.transform.position = ghostPos;
        ghostInstance.transform.rotation = selectedModel.transform.rotation;
        ghostInstance.transform.localScale = selectedModel.transform.localScale;
    }

    private void OnDrawGizmos()
    {
        if (selectedPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(selectedPoint, 1f);
        }
    }
    private void DoModelDragDrop(Ray ray)
    {
        // Mouse button pressed - attempt to pick up a Model
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                // Try to get the Model component on the hit object (or its parents)
                Model hitModel = null;
                var go = hitInfo.collider.gameObject;
                if (!go.TryGetComponent<Model>(out hitModel))
                {
                    // check parent chain in case collider is child
                    hitModel = go.GetComponentInParent<Model>();
                }

                if (hitModel != null)
                {
                    BeginDrag(hitModel, hitInfo.point, ray);
                }
                else
                {
                    Debug.Log("Hit non-model: " + hitInfo.collider.gameObject.name);
                }
            }
        }

        // While holding mouse button - drag the selected Model
        if (selectedModel != null && Input.GetMouseButton(0))
        {
            DragSelected(ray);
        }

        // Mouse button released - drop the Model
        if (selectedModel != null && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void BeginDrag(Model model, Vector3 hitPoint, Ray ray)
    {
        selectedModel = model;
        selectedRb = selectedModel.GetComponent<Rigidbody>();
        if (selectedRb != null)
        {
            // make kinematic so physics doesn't fight direct transform changes
            selectedRb.isKinematic = true;
        }

        // Create a drag plane parallel to the tabletop (horizontal) at the model's current height
        dragPlane = new Plane(Vector3.up, selectedModel.transform.position);

        // offset so the piece doesn't jump to be center-on-hitpoint
        dragOffset = selectedModel.transform.position - hitPoint;

        // compute pickup height relative to the cube beneath (if any) so we can snap back with same vertical offset
        var currentCube = selectedModel.CurrentCube;
        if (currentCube != null)
        {
            pickUpHeightFromCube = selectedModel.transform.position.y - currentCube.worldPosition.y;
        }
        else
        {
            // fallback: preserve world Y delta from drag plane origin
            pickUpHeightFromCube = 1f;
        }

        // Optionally change appearance while dragging (e.g., tint). Keep simple for now.
        // Debug.Log($"Begin dragging {selectedModel.name}");
    }
    private void DragSelected(Ray ray)
    {
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPointOnPlane = ray.GetPoint(enter);
            Vector3 targetPos = hitPointOnPlane + dragOffset;

            // Keep the model above the plane by its pickUpHeightFromCube relative to the plane's y (plane was created at model's start Y)
            targetPos.y = Mathf.Max(targetPos.y, dragPlane.distance > 0 ? -dragPlane.distance : 0); // defensive, though not strictly necessary

            selectedModel.transform.position = targetPos;
        }
    }
    private void EndDrag()
    {
        // Snap onto cube if possible
        //Cube cube = GetCube(selectedModel.transform.position);
        //if (cube != null)
        //{
        //    Vector3 snapped = new Vector3(cube.worldPosition.x, cube.worldPosition.y + pickUpHeightFromCube, cube.worldPosition.z);
        //    selectedModel.transform.position = snapped;

        //    // Inform the model which cube it is on
        //    selectedModel.ChangeCube(cube);
        //}
        //else
        //{
        //    // If no cube under, leave at current position
        //    Debug.Log("Dropped model but no cube found to snap to.");
        //    selectedModel.ChangeCube(null);
        //}

        // restore physics
        if (selectedRb != null)
        {
            selectedRb.isKinematic = false;
        }

        // Clear selection
        selectedModel = null;
        selectedRb = null;
    }
}
