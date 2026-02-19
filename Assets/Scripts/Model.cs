using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour
{
    [Header("Events")]
    public GameEvent onModelMoved;

    [Header("Model Details")]
    public Tabletop tabletop;

    public Unit unit;
    private GameObject basePrefab;
    private GameObject hitBox;

    private Cube cubeIn;

    private Vector3 lastPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        basePrefab = this.transform.GetChild(0).gameObject;
        hitBox = this.transform.GetChild(1).gameObject;

        tabletop = FindObjectOfType<Tabletop>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tabletop == null) return;
        if (unit == null) return;

        if (lastPosition != this.transform.localPosition)
        {
            Debug.Log("Model moved to: " + this.transform.localPosition);
            onModelMoved.Raise(this, this.transform.localPosition);
            lastPosition = this.transform.localPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if (unit != null && basePrefab != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, unit.baseSizeMM / 2);
        }

        if (cubeIn != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(cubeIn.worldPosition, cubeIn.worldSize);
        }
    }
}
