using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
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

    void Update()
    {
        switch (State)
        {
            case GameState.Lobby:
                CurrentScene = "Scene_Lobby";
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

    void ChangeState(GameState newState)
    {
        State = newState;
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
