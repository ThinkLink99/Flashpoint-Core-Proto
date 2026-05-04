using System.Xml.Linq;
using UnityEditor;
public class TurnStartState : BaseState
{
    public TurnStartState(PlayerController playerController) : base(playerController) { }

    public override void OnEnter()
    {
        playerController.BeginTurn();
    }
    public override void Update()
    {
    }
}
