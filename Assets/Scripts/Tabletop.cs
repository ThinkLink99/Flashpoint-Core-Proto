using Assets.Scripts;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Tabletop : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Events")]
    [SerializeField] private GameEvent onGameStart;
    [SerializeField] private GameEvent onModelSelected;
    [SerializeField] private GameEvent onModelDeselected;
    [SerializeField] private GameEvent onModelMoveDeactivated;

    [Header("Point-Click Movement")]
    [SerializeField] private bool movingModel = false;
    [SerializeField] private Model selectedModel;
    [SerializeField] private Vector3 selectedPoint = Vector3.zero;
    private GameObject ghostInstance; // Ghost preview instance

    [SerializeField] private float cubeSize = 76.2f; // world units per cube
    private LineRenderer rangeRenderer;
    private int circleSegments = 64;


    private Map currentMap;

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
         DoModelPointAndClickMove(mousePos, ray);
    }

    public void EnableModelMovementMode ()
    {
        movingModel = true;
    }
    public void DisableeModelMovementMode()
    {
        movingModel = false;
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
                    if (selectedModel != null) onModelSelected?.Raise(this, selectedModel);
                }
                else
                {
                    selectedModel = hitModel;
                    onModelSelected?.Raise(this, selectedModel);
                }
            }
            return; // only attempt to select on initial click, don't also select and set point in same frame
        }

        if (movingModel && selectedModel != null && Input.GetMouseButtonDown(0))
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
            var prevSelected = selectedModel;

            // cancel movement
            selectedPoint = Vector3.zero;
            selectedModel = null;

            // ensure ghost removed if selection cancelled
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }

            onModelDeselected?.Raise(this, prevSelected);
            onModelMoveDeactivated?.Raise(this, prevSelected);
            DisableeModelMovementMode ();
        }

        // Update or create ghost preview based on current selected point/selection
        ShowModelAsGhost();
    }
    private void ShowModelAsGhost()
    {
        // If there is no selection or no valid target point, remove any existing ghost and return
        if (selectedModel == null || selectedPoint == Vector3.zero)
        {
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            HideRangeCircle();
            return;
        }

        // Compute clamped point and in-range status for visual feedback
        Vector3 clampedPoint = ClampPointToMoveRange(selectedPoint);
        bool inRange = false;
        if (selectedModel.CurrentCube != null && selectedModel.unit != null)
        {
            float maxDistanceWorld = selectedModel.unit.unitAdvanceSpeed * cubeSize;
            inRange = Vector3.Distance(selectedModel.CurrentCube.worldPosition, clampedPoint) <= maxDistanceWorld + 0.01f;
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
        }

        // Tint and transparency for the ghost (green if in-range, red if out-of-range)
        Color tint = inRange ? new Color(0f, 0.8f, 0f, 0.45f) : new Color(0.9f, 0f, 0f, 0.45f);

        foreach (var rend in ghostInstance.GetComponentsInChildren<Renderer>())
        {
            var mats = rend.sharedMaterials;
            Material[] newMats = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                Material baseMat = mats[i] != null ? new Material(mats[i]) : new Material(Shader.Find("Standard"));

                // apply tint while preserving alpha
                if (baseMat.HasProperty("_Color"))
                {
                    Color c = baseMat.color;
                    c.r = tint.r;
                    c.g = tint.g;
                    c.b = tint.b;
                    c.a = tint.a;
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

        // Ensure there is a small ring under the ghost to indicate the tile it will occupy
        const string ringName = "GhostRing";
        Transform ringTf = ghostInstance.transform.Find(ringName);
        LineRenderer ring = null;
        if (ringTf == null)
        {
            var ringGo = new GameObject(ringName);
            ringGo.transform.SetParent(ghostInstance.transform, false);
            ring = ringGo.AddComponent<LineRenderer>();
            ring.loop = true;
            ring.positionCount = 32;
            ring.useWorldSpace = false;
            ring.startWidth = 0.02f;
            ring.endWidth = 0.02f;
            ring.material = new Material(Shader.Find("Sprites/Default"));
            // place ring slightly below model center so it sits on the table
            ringGo.transform.localPosition = new Vector3(0f, -0.01f, 0f);
        }
        else
        {
            ring = ringTf.GetComponent<LineRenderer>();
        }

        if (ring != null)
        {
            float ringRadius = cubeSize * 0.5f;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float t = i / (float)ring.positionCount;
                float ang = t * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(ang) * ringRadius, 0f, Mathf.Sin(ang) * ringRadius);
                ring.SetPosition(i, p);
            }
            ring.startColor = tint;
            ring.endColor = tint;
            ring.enabled = true;
        }

        // Position and orient the ghost at the clamped point.
        // Preserve the model's original vertical offset from its cube if possible
        float yOffset = 0f;
        if (selectedModel.CurrentCube != null)
        {
            yOffset = selectedModel.transform.position.y - selectedModel.CurrentCube.worldPosition.y;
        }
        Vector3 ghostPos = new Vector3(clampedPoint.x, clampedPoint.y + yOffset, clampedPoint.z);
        ghostInstance.transform.position = ghostPos;
        ghostInstance.transform.rotation = selectedModel.transform.rotation;
        ghostInstance.transform.localScale = selectedModel.transform.localScale;

        // Draw full-movement-range circle around origin cube for reference
        if (selectedModel.CurrentCube != null && selectedModel.unit != null)
        {
            DrawRangeCircle(selectedModel.CurrentCube.worldPosition, selectedModel.unit.unitAdvanceSpeed * cubeSize);
        }
    }

    private void GetMovementRange (Model model)
    {
        // Optionally, calculate and visualize the valid movement range for the selected model based on its current position, unit stats, and any obstacles on the tabletop.
        // This can help players understand where they can move the piece before selecting a target point.

        // Get the world position of the models current cube
        var currentPos = model.CurrentCube.mapPosition;

        // Use the model's unit stats to determine movement range (e.g., how many tiles it can move)
        var advance = model.unit.unitAdvanceSpeed;
        var sprint = model.unit.unitSprintSpeed;

        // Perform a breadth-first search or similar algorithm to find all reachable tiles within the movement range, taking into account obstacles and other pieces


        // Visualize the valid movement range, for example by highlighting the reachable tiles or showing a radius around the piece.

    }
    public Cube GetCube(Vector3 position)
    {
        for (int x = 0; x < currentMap.MapSize.X; x++)
        {
            for (int y = 0; y < currentMap.MapSize.Y; y++)
            {
                for (int z = 0; z < currentMap.MapSize.Z; z++)
                {
                    var cube = currentMap.MapGrid.Get(x, y, z);
                    if (cube is null) continue;

                    if (cube.PositionIsInCube(position))
                    {
                        return cube;
                    }
                }
            }
        }

        // We found nothing
        Debug.LogError("GetCube: No cube found at position " + position.ToString());
        return null;
    }

    private void EnsureRangeRenderer()
    {
        if (rangeRenderer != null) return;
        var go = new GameObject("RangeRenderer");
        go.transform.SetParent(transform);
        rangeRenderer = go.AddComponent<LineRenderer>();
        rangeRenderer.positionCount = circleSegments + 1;
        rangeRenderer.loop = true;
        rangeRenderer.useWorldSpace = true;
        rangeRenderer.startWidth = 0.05f;
        rangeRenderer.endWidth = 0.05f;
        rangeRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1f, 1f, 1f, 0.25f) };
    }

    private void DrawRangeCircle(Vector3 center, float radius)
    {
        EnsureRangeRenderer();
        for (int i = 0; i <= circleSegments; i++)
        {
            float t = i / (float)circleSegments;
            float ang = t * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius) + center;
            rangeRenderer.SetPosition(i, p);
        }
        rangeRenderer.gameObject.SetActive(true);
    }

    private void HideRangeCircle()
    {
        if (rangeRenderer != null) rangeRenderer.gameObject.SetActive(false);
    }

    // Use this when updating the ghost/preview. This clamps the selectedPoint to the unit's move range.
    private Vector3 ClampPointToMoveRange(Vector3 desiredWorldPoint)
    {
        if (selectedModel == null || selectedModel.CurrentCube == null || selectedModel.unit == null) return desiredWorldPoint;

        float maxDistanceWorld = selectedModel.unit.unitAdvanceSpeed * cubeSize;
        Vector3 origin = selectedModel.CurrentCube.worldPosition;
        float dist = Vector3.Distance(origin, desiredWorldPoint);

        if (dist <= maxDistanceWorld) return desiredWorldPoint;

        // Clamp along direction, then snap to nearest cube (using your GetCubeAtWorldPoint)
        Vector3 clamped = origin + (desiredWorldPoint - origin).normalized * maxDistanceWorld;
        var clampedCube = GetCube(clamped); // implement or reuse your cube lookup
        return clampedCube != null ? clampedCube.worldPosition : clamped;
    }

    public void OnMapCreated (Component sender, object data)
    {
        if (data is Map map)
        {
            currentMap = map;
        }
    }
    private void OnDrawGizmos()
    {
        if (selectedPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(selectedPoint, 1f);
        }
    }
}
