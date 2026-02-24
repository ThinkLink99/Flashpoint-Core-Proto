using UnityEngine;

public class GhostPreviewManager : MonoBehaviour
{
    // simple single-instance ghost with basic material tinting
    private GameObject ghostInstance;

    public void ShowGhost(GameObject sourcePrefab, Vector3 pos, Quaternion rot, bool inRange)
    {
        if (sourcePrefab == null) return;

        if (ghostInstance == null)
        {
            ghostInstance = Instantiate(sourcePrefab);
            ghostInstance.name = sourcePrefab.name + "_Ghost";

            var modelComp = ghostInstance.GetComponent<Model>();
            if (modelComp != null) Destroy(modelComp);

            foreach (var col in ghostInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;

            foreach (var rb in ghostInstance.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);

            int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreLayer != -1)
            {
                foreach (Transform t in ghostInstance.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = ignoreLayer;
            }
        }

        // apply tint
        Color tint = inRange ? new Color(0f, 0.8f, 0f, 0.45f) : new Color(0.9f, 0f, 0f, 0.45f);
        ApplyTint(ghostInstance, tint);

        // ensure ring
        EnsureRing(ghostInstance, tint);

        // position
        ghostInstance.transform.position = pos;
        ghostInstance.transform.rotation = rot;
        // keep source scale
        ghostInstance.transform.localScale = sourcePrefab.transform.localScale;
    }

    public void HideGhost()
    {
        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }
    }

    private void ApplyTint(GameObject ghost, Color tint)
    {
        foreach (var rend in ghost.GetComponentsInChildren<Renderer>())
        {
            var shared = rend.sharedMaterials;
            Material[] newMats = new Material[shared.Length];
            for (int i = 0; i < shared.Length; i++)
            {
                Material baseMat = shared[i] != null ? new Material(shared[i]) : new Material(Shader.Find("Standard"));
                if (baseMat.HasProperty("_Color"))
                {
                    Color c = baseMat.color;
                    c.r = tint.r; c.g = tint.g; c.b = tint.b; c.a = tint.a;
                    baseMat.color = c;
                }

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

    private void EnsureRing(GameObject ghost, Color tint)
    {
        const string ringName = "GhostRing";
        Transform ringTf = ghost.transform.Find(ringName);
        LineRenderer ring = null;
        if (ringTf == null)
        {
            var ringGo = new GameObject(ringName);
            ringGo.transform.SetParent(ghost.transform, false);
            ring = ringGo.AddComponent<LineRenderer>();
            ring.loop = true;
            ring.positionCount = 32;
            ring.useWorldSpace = false;
            ring.startWidth = 0.02f;
            ring.endWidth = 0.02f;
            ring.material = new Material(Shader.Find("Sprites/Default"));
            ringGo.transform.localPosition = new Vector3(0f, -0.01f, 0f);
        }
        else
        {
            ring = ringTf.GetComponent<LineRenderer>();
        }

        if (ring != null)
        {
            float ringRadius = 0.5f; // will be adjusted by caller if needed
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
    }
}