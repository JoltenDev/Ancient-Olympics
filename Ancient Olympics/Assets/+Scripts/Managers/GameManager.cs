using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Lobby,
        Started,
        Joust,
        Assassination,
        JavelinThrow,
        SwordFight
    }

    public static GameState State { get; private set; }
    public static string CurrentScene { get; private set; }

    void Start()
    {
        ChangeState(GameState.Lobby);
    }

    /// <summary>
    /// Controls the state of the game
    /// </summary>
    /// <param name="newState"> State to be swapped into </param>
    void ChangeState(GameState newState)
    {
        ActionEvent.onStateChanged?.Invoke();
        State = newState;

        switch (State)
        {
            case GameState.Lobby:
                LobbyState();
                break;
            case GameState.Started:
                break;
            case GameState.Joust:
                break;
            case GameState.Assassination:
                break;
            case GameState.JavelinThrow:
                break;
            case GameState.SwordFight:
                break;
        }
    }

    void LobbyState()
    {
        CurrentScene = "Scene_Lobby";
    }

    public override void OnNetworkSpawn()
    {
        ChangeScene();
    }

    /// <summary>
    /// Handles the scene management
    /// </summary>
    /// <returns> true if the scene successfully loaded, false otherwise </returns>
    bool ChangeScene()
    {
        if (IsServer && !string.IsNullOrEmpty(CurrentScene))
        {
            var status = NetworkManager.SceneManager.LoadScene(CurrentScene, LoadSceneMode.Single);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {CurrentScene} " + $"with a {nameof(SceneEventProgressStatus)}: {status}");
                return false;
            }
        }

        return true;
    }

    public void ApplicationQuit() => Application.Quit();

    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
