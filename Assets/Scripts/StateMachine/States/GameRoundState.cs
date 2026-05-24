using System.Collections.Generic;
using UnityEngine;

public class GameRoundState : BaseGameState
{
    private List<PlayerController> players;
    private int playerTurnIndex = -1;

    private bool turnChanged = false;

    private ModelActionView modelInformationController;
    private GameEvent onTurnStarted;
    private GameEventListener onTurnEndedListener;

    public GameRoundState(GameManager gameManager, ModelActionView modelInformationController) : base(gameManager)
    {
        this.modelInformationController = modelInformationController;
    }

    public override void OnEnter()
    {
        players = gameManager.players;

        NextTurn();
    }
    public override void Update()
    {
        // On Update we need to check whose turn it is and allow that players state machine drive their UI and state. 
        if (turnChanged)
        {
            turnChanged = false;
            Debug.Log("Getting Next Player Turn");
            var currentPlayer = players[playerTurnIndex];
            if (currentPlayer != null)
            {
                currentPlayer.BeginTurn();

                if (currentPlayer.isHumanControlled)
                {
                    // hide while we reconfigure
                    modelInformationController.HideUI();

                    // Configure the view through its public API instead of setting fields directly.
                    // This keeps the view responsible for handling any internal updates when the
                    // controller changes and avoids tightly coupling the state implementation to
                    // the view's internal fields.
                    modelInformationController.SetPlayerController(currentPlayer);

                    modelInformationController.ShowUI();
                }
            }
        }
    }

    public void NextTurn ()
    {
        playerTurnIndex++;
        if (playerTurnIndex >= players.Count) playerTurnIndex = 0;
        
        turnChanged = true;
        onTurnStarted?.Raise(null, players[playerTurnIndex]);
    }
}