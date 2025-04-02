using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System;
using TMPro;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    GameStates states;
    GameBaseState currentState;
    Timer timer = new Timer();

    public GameStates States { get => states; }
    public GameBaseState CurrentState { get => currentState; set => currentState = value; }
    public Timer Timer { get => timer; }

    #region Actions
    public Action onGameStartingStarted;
    public Action onGameStartingCompleted;
    public Action onTransitionStarted;
    public Action onTransitionCompleted;
    public Action onBuffferStarted;
    public Action onBufferCompleted;
    #endregion

    [SerializeField] List<ulong> deadPlayers = new List<ulong>();
    public List<ulong> DeadPlayers { get => deadPlayers; set => deadPlayers = value; }

    void Start()
    {
        states = new GameStates(this);

        Timer.RegisterTimer(Timer.timerStartedEvents, "Game Starting", () => { onGameStartingStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Game Starting", () => { onGameStartingCompleted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerStartedEvents, "Transition", () => { onTransitionStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Transition", () => { onTransitionCompleted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerStartedEvents, "Buffer", () => { onBuffferStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Buffer", () => { onBufferCompleted?.Invoke(); });
    }

    void Update()
    {
        CurrentState?.Update();
    }

    public void SwitchState(GameBaseState newState)
    {
        CurrentState?.Exit(); // Exit current state
        newState?.Enter(); // Enter new state

        CurrentState = newState;
    }

    [Rpc(SendTo.Server)]
    public void RepositionPlayersRpc()
    {
        if (!IsServer) return;

        foreach (var player in NetworkLobbyManager.Instance.playersInServer.Keys)
        {
            float spawn_x = UnityEngine.Random.Range(-3f, 3.25f);
            float spawn_z = UnityEngine.Random.Range(-1.5f, 1.5f);
            Vector3 spawnPos = new Vector3(spawn_x, 0.4f, spawn_z);

            var playerObject = NetworkLobbyManager.Instance.playersInServer[player];
            var currentScale = playerObject.transform.localScale;
            playerObject.GetComponent<NetworkTransform>().Teleport(spawnPos, Quaternion.identity, currentScale);
        }
    }

    [Rpc(SendTo.Server)]
    public void RevivePlayersRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            var player = client.PlayerObject.GetComponentInChildren<Player>();
            var health = client.PlayerObject.GetComponentInChildren<NetworkHealth>();

            player.EnablePlayerRpc();
            player.IdleRpc();
            health.SendHealRpc(health.MaxHealth);
            health.Dead.Value = false;
        }
    }

    [Rpc(SendTo.Server)]
    public void AddPlayerToDeadListRpc(ulong id) => DeadPlayers.Add(id);

    #region Quit
    public void ApplicationQuit() => Application.Quit();
    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    #endregion
}
