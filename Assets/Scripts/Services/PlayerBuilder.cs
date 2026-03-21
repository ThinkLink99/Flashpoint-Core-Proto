using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBuilder
{
    private readonly Transform tabletop;
    private readonly ModelSpawner modelSpawner;

    private string playerName = "Player";
    private TeamId playerTeam = TeamId.Red;
    private Fireteam fireteam;
    
    public PlayerBuilder(Transform tabletop, ModelSpawner modelSpawner)
    {
        this.tabletop = tabletop;
        this.modelSpawner = modelSpawner;
    }
    public PlayerBuilder Start() => this;
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
        var go = new GameObject(playerName);
        go.transform.parent = tabletop;
        var player = go.AddComponent<PlayerController>();
        player.modelSpawner = modelSpawner;
        player.team = playerTeam;
        if (fireteam != null) player.fireteam = fireteam;

        return player;
    }
}
