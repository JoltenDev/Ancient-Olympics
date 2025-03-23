using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    GameStates states;
    GameBaseState currentState;
    Timer timer = new Timer();

    public GameStates States { get { return states; } }
    public GameBaseState CurrentState { get => currentState; set => currentState = value; }
    public Timer Timer { get { return timer; } }

    public Action onTransitionStarted;
    public Action onTransitionCompleted;

    void Start()
    {
        states = new GameStates(this);

        Timer.RegisterTimer(Timer.timerStartedEvents, "Transition", () => { onTransitionStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Transition", () => { onTransitionCompleted?.Invoke(); });
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

    public void ApplicationQuit() => Application.Quit();

    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
