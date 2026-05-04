using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    #region Singleton
    public static Game Instance;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }
    #endregion

    public delegate void GameEventHandler(Component sender, object eventData);

    public event GameEventHandler OnGameStart;
    public void RaiseGameStartEvent(Component sender, object eventData)
    {
        OnGameStart?.Invoke(sender, eventData);
    }

    public event GameEventHandler OnGameStop;
    
    public event GameEventHandler OnGamePause;

    public event GameEventHandler OnMapSelected;
    public event GameEventHandler OnMapCreated;
    public void RaiseMapCreatedEvent(Component sender, object eventData)
    {
        OnMapCreated?.Invoke(sender, eventData);
    }

    public event GameEventHandler OnPlayersCreated;
    public void RaisePlayersCreatedEvent(Component sender, object eventData)
    {
        OnPlayersCreated?.Invoke(sender, eventData);
    }

    public event GameEventHandler OnAllPlayersDeployed;
    public void RaiseAllPlayersDeployedEvent(Component sender, object eventData)
    {
        OnAllPlayersDeployed?.Invoke(sender, eventData);
    }

    // Game needs a reference to the chosen map, the players, and the current round
    public Map CurrentMap;
    public GameInfoSO GameInfo;
    public List<PlayerController> Players;
    public int Round = 0;

    public static void LoadScene (int sceneIndex)
    {
        var activeScene = SceneManager.GetActiveScene();
        SceneManager.UnloadSceneAsync(activeScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }
}