using UnityEngine;
using Unity.Netcode;

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

    public GameState State { get; private set; }

    public void ChangeState(GameState newState)
    {
        State = newState;

        switch (State)
        {
            case GameState.Lobby:
                LobbyState();
                break;
            case GameState.Started:
                StartedState();
                break;
        }
    }

    void LobbyState()
    {
        // Load the lobby scene and instantiate the lobby menu
        NetworkSceneManager.Instance.ChangeScene("Scene_Lobby", () =>
        {
            if (IsServer)
            {
                ActionEvent.onGameStarted += RepositionPlayers;
                //UIManager.Instance.UpdateLobbyUI();
            }
        });
    }

    void StartedState()
    {
        // Load the arena scene and handle other game state transitions
        NetworkSceneManager.Instance.ChangeScene("Scene_Arena", () =>
        {
            ActionEvent.onGameStarted?.Invoke();
        });
    }

    public void RepositionPlayers()
    {
        foreach (var player in NetworkLobbyManager.Instance.playersInServer.Values)
        {
            float spawn_x = Random.Range(-3f, 3.25f);
            float spawn_z = Random.Range(-1.5f, 1.5f);
            Vector3 spawnPos = new Vector3(spawn_x, 0.4f, spawn_z);

            player.GetComponent<Rigidbody>().position = spawnPos;
        }
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
