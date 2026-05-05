using System.Collections.Generic;
using UnityEngine;

public class GameRoundState : BaseGameState
{
    private List<PlayerController> players;
    private int playerTurnIndex = -1;

    private bool turnChanged = false;

    private ModelInformationController modelInformationController;
    private GameEvent onTurnStarted;
    private GameEventListener onTurnEndedListener;

    public GameRoundState(GameManager gameManager, ModelInformationController modelInformationController) : base(gameManager)
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
                    // show UI
                    modelInformationController.playerController = currentPlayer;
                    modelInformationController.ControllerChanged();

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