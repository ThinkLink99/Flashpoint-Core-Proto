using UnityEngine;

public class PlayerBuilder : MonoBehaviour
{
    public void OnGameStart (Component sender, object data)
    {
        CreatePlayer();
    }

    public Player CreatePlayer ()
    {
        var go = new GameObject("Player");
        go.transform.parent = this.transform;
        go.AddComponent<Player>();

        return go.GetComponent<Player>();
    }
}
