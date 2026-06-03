using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public void OnAwake ()
    {
        playerBuilder = this.AddComponent<PlayerBuilder>();
    }
    private PlayerBuilder playerBuilder;
    [SerializeField] private ModelConfiguration[] testModelsToSpawn;

    public IEnumerator<List<PlayerController>> CreatePlayerObjects(params PlayerOptions[] playerOptions)
    {
        var playersCreated = new List<PlayerController>();
        foreach (var playerOption in playerOptions)
        {
            var builder = playerBuilder
                .Start()
                .AddFireteam(testModelsToSpawn)
                .SetName(playerOption.playerName)
                .SetTeam(playerOption.teamId);
            if (playerOption.isHuman)
                builder.IsHuman();
            else builder.IsHuman(false);

            PlayerController player = builder.Build();
            if (player.isHumanControlled) player.IsLocalPlayer = true;
            playersCreated.Add(player);
        }

        yield return playersCreated;
    }
}