using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class AIPlayerController : MonoBehaviour
{
    private PlayerController owner;
    private GameManager gameManager;

    void Awake()
    {
        owner = GetComponent<PlayerController>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public void BeginTurn()
    {
        StartCoroutine(RunAILogic());
    }

    private IEnumerator RunAILogic()
    {
        // small delay to simulate thinking / allow UI to update
        yield return new WaitForSeconds(0.5f);

        // Simple example: iterate activations and take trivial actions
        var activations = new System.Collections.Generic.List<Model>(owner.ActivationsRemaining);
        foreach (var model in activations)
        {
            if (model == null) continue;

            // select model (server-authoritative call in a real networked app)
            gameManager.SelectModel(model);

            // choose a destination or target (example: stay in place or random nearby)
            Vector3 dest = model.transform.position; // replace with actual decision logic
            gameManager.SelectDestination(dest);

            // Try to execute an AdvanceAction or ShootAction depending on logic. Here we create a simple AdvanceAction.
            // The AI must use the same action classes you use for players.
            var action = new AdvanceAction(new MovementPlanner(gameManager.Context)); // adapt as needed
            if (gameManager.TryExecuteAction(action))
            {
                // wait a bit for animation / action to progress
                yield return new WaitForSeconds(0.6f);
            }
            else
            {
                yield return null;
            }
        }

        // End turn on owner so GameManager/turn system continues
        owner.EndTurn();
    }
}