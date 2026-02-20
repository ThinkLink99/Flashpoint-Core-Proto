using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tabletop : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Events")]
    [SerializeField] private GameEvent onGameStart;

    // Start is called before the first frame update
    void Start()
    {
        onGameStart.Raise(this, null);
    }

    // Update is called once per frame
    void Update()
    {
        var mousePos = Input.mousePosition;
        var worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.transform.position.y));

        if (Input.GetMouseButtonDown(0))
        {
            var ray = mainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Debug.Log("Hit: " + hitInfo.collider.gameObject.name);
            }
        }
    }
}
