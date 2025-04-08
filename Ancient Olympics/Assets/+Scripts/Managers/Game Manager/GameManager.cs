using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] NetworkObject npc;

    GameStates states;
    GameBaseState currentState;
    Timer timer = new Timer();

    List<NetworkObject> spawnedNpcs = new List<NetworkObject>();
    [SerializeField] NetworkList<ulong> deadPlayers = new NetworkList<ulong>();
    NetworkVariable<float> javelinSpeedAddend = new NetworkVariable<float>();

    public string startMessage;

    #region Getters/Setters
    public GameStates States { get => states; }
    public Timer Timer { get => timer; }
    public NetworkList<ulong> DeadPlayers { get => deadPlayers; }
    public GameBaseState CurrentState { get => currentState; set => currentState = value; }
    public NetworkVariable<float> JavelinSpeedAddend { get => javelinSpeedAddend; }
    #endregion

    #region Actions
    public Action onGameStartingStarted;
    public Action onGameStartingCompleted;
    public Action onRoundActiveStarted;
    public Action onRoundActiveCompleted;

    public Action onTransitionStarted;
    public Action onTransitionCompleted;
    public Action onBuffferStarted;
    public Action onBufferCompleted;
    #endregion

    void Start()
    {
        states = new GameStates(this);

        Timer.RegisterTimer(Timer.timerStartedEvents, "Game Starting", () => { onGameStartingStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Game Starting", () => { onGameStartingCompleted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerStartedEvents, "Round Active", () => { onRoundActiveStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Round Active", () => { onRoundActiveCompleted?.Invoke(); });

        Timer.RegisterTimer(Timer.timerStartedEvents, "Transition", () => { onTransitionStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Transition", () => { onTransitionCompleted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerStartedEvents, "Buffer", () => { onBuffferStarted?.Invoke(); });
        Timer.RegisterTimer(Timer.timerCompletedEvents, "Buffer", () => { onBufferCompleted?.Invoke(); });

        onRoundActiveCompleted += RoundTimerCompleted;
        deadPlayers.OnListChanged += CheckPlayerCount;
    }

    public override void OnDestroy()
    {
        onRoundActiveCompleted -= RoundTimerCompleted;
    }

    void Update()
    {
        CurrentState?.Update();

        if (Timer.timerRemainingTimes.ContainsKey("Game Starting"))
        {
            float time = Timer.timerRemainingTimes["Game Starting"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"{startMessage} <color=#2E2E2E>has been chosen!\nStarting... {time:F1}s");
        }

        if (Timer.timerRemainingTimes.ContainsKey("Round Active"))
        {
            float time = Timer.timerRemainingTimes["Round Active"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"Time left: {time:F1}s");
        }
    }

    /// <summary>
    /// Transitions from the current game state to a new game state.
    /// Calls the <c>Exit</c> method on the current state (if any),
    /// then calls the <c>Enter</c> method on the new state.
    /// </summary>
    /// <param name="newState">The new game state to switch to.</param>
    public void SwitchState(GameBaseState newState)
    {
        CurrentState?.Exit(); // Exit current state
        newState?.Enter(); // Enter new state

        CurrentState = newState;
    }

    /// <summary>
    /// Checks the number of dead and connected players to determine if only one player remains alive.
    /// </summary>
    /// <param name="changeEvent">The event triggered by a change in the network list of connected players.</param>
    void CheckPlayerCount(NetworkListEvent<ulong> changeEvent)
    {
        // This is invoked twice, may cause future problems

        if (!IsServer) return;
        if (currentState == states.GameTransitionState()) return;

        if (DeadPlayers.Count == NetworkManager.Singleton.ConnectedClients.Count - 1) // If the amount of dead players is equal to the amount of connect clients - 1
        {
            ulong alivePlayerId = 100;

            foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                if (!DeadPlayers.Contains(id)) // Check if client is not in DeadPlayers
                {
                    alivePlayerId = id; // Store the alive player ID
                    break;
                }
            }

            Player player = NetworkManager.Singleton.ConnectedClients[alivePlayerId].PlayerObject.GetComponentInChildren<Player>();
            UIManager.Instance.UpdatePlayerTimersRpc($"<color=#59dac9>{player.Username} <color=#2E2E2E>has won the round!"); // Update Message

            SwitchState(states.GameTransitionState());
        }
    }

    void RoundTimerCompleted()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Time is finished!\n<color=#b22020>Nobody <color=#2E2E2E>has won the round!"); // Update Message
        SwitchState(states.GameTransitionState());
    }

    /// <summary>
    /// Assigns a weapon to every connected player in the game.
    /// <para>Weapon IDs: 0 = Sword, 1 = Knife, 2 = Javelin, 3 = Lance</para>
    /// </summary>
    /// <param name="weapon">The ID of the weapon to assign to each player.</param>
    /// <param name="unequip">If true, unequips the weapon from all players instead of equipping it.</param>
    /// <param name="hidden">If true, the weapon will not be visible to other players.</param>
    public void AssignWeaponToEveryPlayer(int weapon, bool unequip = false, bool hidden = false)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            var weaponHandler = client.PlayerObject.GetComponent<WeaponHandler>();

            if (!unequip)
                weaponHandler.EquipWeaponRpc(client.ClientId, weapon, hidden);
            else
                weaponHandler.UnequipWeaponRpc();
        }
    }

    #region Rpcs
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
            var health = client.PlayerObject.GetComponentInChildren<NetworkHealth>();
            health.SendHealRpc(health.MaxHealth);
        }
    }

    [Rpc(SendTo.Server)]
    public void AddPlayerToDeadListRpc(ulong id) => DeadPlayers.Add(id);

    [Rpc(SendTo.Server)]
    public void SpawnNpcsRpc(int amount)
    {
        if (!IsServer) return;

        for (int i = 0; i < amount; i++)
        {
            float spawn_x = UnityEngine.Random.Range(-3f, 3.25f);
            float spawn_z = UnityEngine.Random.Range(-1.5f, 1.5f);
            Vector3 spawnPos = new Vector3(spawn_x, 0.4f, spawn_z);

            var clone = npc.InstantiateAndSpawn(NetworkManager.Singleton, 120, true, false, false, spawnPos, Quaternion.identity);
            spawnedNpcs.Add(clone);
        }
    }

    [Rpc(SendTo.Server)]
    public void DespawnAllNpcsRpc()
    {
        if (!IsServer) return;

        foreach (var clone in spawnedNpcs) 
        {
            clone.Despawn(true);
        }

        spawnedNpcs.Clear();
    }

    [Rpc(SendTo.Server)]
    public void SetJavelinAddendRpc(float amount)
    {
        if (!IsServer) return;

        javelinSpeedAddend.Value = amount;
    }
    #endregion

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
