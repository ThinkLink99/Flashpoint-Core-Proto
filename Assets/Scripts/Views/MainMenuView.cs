using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameEvent onGameStart;

    private Button startButton;

    private Button playerSelectionContinue = null;
    private Button playerSelectionBack = null;

    private VisualElement menuOptions = null;
    private VisualElement playerSelection = null;

    private TextField Player1Name = null;
    private TextField Player2Name = null;
    private EnumField Player1Team = null;
    private EnumField Player2Team = null;
    private Toggle Player1IsAI = null;
    private Toggle Player2IsAI = null;

    private PlayerOptions player1Options;
    private PlayerOptions player2Options;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        startButton = root.Q<Button>("Start");

        menuOptions = root.Q("MenuOptions");
        playerSelection = root.Q("PlayerSelection");

        playerSelectionContinue = root.Q<Button>("btnContinue");
        playerSelectionBack = root.Q<Button>("btnBack");

        Player1Name = root.Q<TextField>("Player1Name");
        Player1Team = root.Q<EnumField>("Player1Team");
        Player1IsAI = root.Q<Toggle>("Player1IsAI");

        Player2Name = root.Q<TextField>("Player2Name");
        Player2Team = root.Q<EnumField>("Player2Team");
        Player2IsAI = root.Q<Toggle>("Player2IsAI");

        startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
        playerSelectionContinue.RegisterCallback<ClickEvent>(OnPlayerSelectionContinueClicked);
        playerSelectionBack.RegisterCallback<ClickEvent>(OnPlayerSelectionBackClicked);
    }

    private void OnStartButtonClicked(ClickEvent e)
    {
        menuOptions.style.display = DisplayStyle.None;
        playerSelection.style.display = DisplayStyle.Flex;
    }
    private void OnPlayerSelectionContinueClicked(ClickEvent e)
    {
        playerSelection.style.display = DisplayStyle.None;

        //onGameStart?.Raise(this, e);
        player1Options = new PlayerOptions
        {
            playerName = Player1Name.value,
            teamId = (TeamId)Player1Team.value,
            isHuman = !Player1IsAI.value
        };
        player2Options = new PlayerOptions
        {
            playerName = Player2Name.value,
            teamId = (TeamId)Player2Team.value,
            isHuman = !Player2IsAI.value
        };


        //Game.Instance.GameInfo.PlayerOptions = new List<PlayerOptions> { player1Options, player2Options };

        //Game.LoadScene(2); // Load the Game Scene
    }

    private void OnPlayerSelectionBackClicked(ClickEvent e)
    {
        menuOptions.style.display = DisplayStyle.Flex;
        playerSelection.style.display = DisplayStyle.None;
    }
}
