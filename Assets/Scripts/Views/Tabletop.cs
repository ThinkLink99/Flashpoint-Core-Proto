using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tabletop : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] private TabletopCamera tabletopCameraController;

    [Header("Players")]
    [SerializeField] public List<PlayerController> players;

    public Map currentMap;

    void Start()
    {
        if (tabletopCameraController == null) mainCamera.TryGetComponent<TabletopCamera>(out tabletopCameraController);
    }
    void Update()
    {

    }

    public void OnMapCreated(Component sender, object data)
    {
        if (data is Map map)
        {
            currentMap = map;
        }
    }
    public void OnPlayersCreated (Component sender, object data)
    {
        Debug.Log("Hit");
        if (data is List<PlayerController> players)
        {
            this.players = players;
        }
    }
}
