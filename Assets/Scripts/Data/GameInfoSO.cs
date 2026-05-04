using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Game Info", menuName = "Game Info")]
public class GameInfoSO : ScriptableObject
{
    [SerializeField] private Model selectedModel;
    [SerializeField] private bool isModelSelected = false;
    [SerializeField] private StyleEnum<DisplayStyle> displaySelectedModelInfo = DisplayStyle.None;

    public Model SelectedModel
    {
        get => selectedModel; 
        set 
        { 
            selectedModel = value; 
            isModelSelected = value != null; 
            displaySelectedModelInfo = isModelSelected ? DisplayStyle.Flex : DisplayStyle.None; }
    }

    public List<PlayerOptions> PlayerOptions;
    public PlayerController CurrentPlayerTurn;

    public int RoundNumber = 0;

    public int Player1Score = 0;
    public int Player2Score = 0;
}
