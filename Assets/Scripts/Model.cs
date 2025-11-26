using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour
{
    public Tabletop tabletop;

    public Unit unit;
    private GameObject basePrefab;
    private GameObject hitBox;

    private Cube cubeIn;

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

        this.transform.localScale = new Vector3(unit.baseSizeMM, 1, unit.baseSizeMM);

        // get tabletop tile based on world position
        cubeIn = tabletop.GetCube(this.transform.position);
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
