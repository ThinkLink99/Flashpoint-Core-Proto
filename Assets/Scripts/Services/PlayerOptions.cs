using UnityEngine;

[CreateAssetMenu(fileName = "PlayerOptions", menuName = "ScriptableObjects/PlayerOptions", order = 1)]
public class PlayerOptions : ScriptableObject
{
    public string playerName;
    public TeamId teamId;
    public bool isHuman;
}
