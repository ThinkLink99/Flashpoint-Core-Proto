using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PlayerBuilder : MonoBehaviour
{
    [SerializeField] private Transform tabletop;
    [SerializeField] private ModelSpawner modelSpawner;

    [SerializeField] private GameObject playerPrefab;

    private string playerName = "Player";
    private bool isHuman = false;
    private TeamId playerTeam = TeamId.Red;
    private Fireteam fireteam;
    
    public PlayerBuilder(Transform tabletop, ModelSpawner modelSpawner)
    {
        this.tabletop = tabletop;
        this.modelSpawner = modelSpawner;
    }
    public PlayerBuilder Start()
    {
        playerName = string.Empty;
        isHuman = false;
        playerTeam = TeamId.Red;
        fireteam = null;

        return this;
    }
    public PlayerBuilder SetName (string name)
    {
        playerName = name;
        return this;
    }
    public PlayerBuilder IsHuman (bool isHuman = true)
    {
        this.isHuman = isHuman;
        return this;
    }
    public PlayerBuilder SetTeam (TeamId team)
    {
        playerTeam = team;

        return this;
    }
    public PlayerBuilder AddFireteam (params ModelConfiguration[] models)
    {
        fireteam = new Fireteam();
        fireteam.Models = models.ToList();

        return this;
    }
    public PlayerController Build ()
    {
        var go = Instantiate(playerPrefab);
        go.name = playerName;
        go.transform.parent = tabletop;
        var player = go.GetComponent<PlayerController>();
        player.modelSpawner = modelSpawner;
        player.isHumanControlled = isHuman;
        player.team = playerTeam;
        if (fireteam != null) player.fireteam = fireteam;
        return player;
    }
}
