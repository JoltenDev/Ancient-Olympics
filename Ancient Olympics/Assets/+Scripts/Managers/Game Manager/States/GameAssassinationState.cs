using Unity.Netcode;
using UnityEngine;

public class GameAssassinationState : GameBaseState
{
    ulong alivePlayerId = 0;

    public GameAssassinationState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StartTimer("Game Starting", 10);

        gameManager.SpawnNpcsRpc(15);
        HideUsernames();

        // Start game
        gameManager.onGameStartingCompleted += BeginGame;
    }

    public override void Update()
    {
        if (gameManager.Timer.timerRemainingTimes.ContainsKey("Game Starting"))
        {
            float time = gameManager.Timer.timerRemainingTimes["Game Starting"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"<color=#ff6666>Assassination <color=#ffffff>has been chosen!\nStarting... {time:F1}s");
        }

        SwitchState(); // Switch state if only 1 player is alive
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.onGameStartingCompleted -= BeginGame;

        RemoveWeapons();
        HideUsernames(false);
        gameManager.DespawnAllNpcsRpc();

        Player player = NetworkManager.Singleton.ConnectedClients[alivePlayerId].PlayerObject.GetComponentInChildren<Player>();
        UIManager.Instance.UpdatePlayerTimersRpc($"<color=#59dac9>{player.Username} <color=#ffffff>has won the round!"); // Update Message
    }

    void BeginGame()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Begin!"); // Update Message

        GiveEveryPlayerWeapon();
    }

    void SwitchState()
    {
        if (GameManager.Instance.DeadPlayers.Count == NetworkManager.Singleton.ConnectedClients.Count - 1)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                if (!GameManager.Instance.DeadPlayers.Contains(client.Key)) // Check if client is not in DeadPlayers
                {
                    alivePlayerId = client.Key; // Store the alive player ID
                    break;
                }
            }

            SwitchState(states.GameTransitionState());
        }
    }

    void HideUsernames(bool hide = true)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (hide)
                client.PlayerObject.GetComponentInParent<Player>().SetPlayerUIRpc("");
            else
                client.PlayerObject.GetComponentInParent<Player>().SetPlayerUIRpc(client.PlayerObject.GetComponentInParent<Player>().Username);
        }
    }

    void GiveEveryPlayerWeapon()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            client.PlayerObject.GetComponent<WeaponHandler>().EquipWeaponRpc(client.ClientId, 1, true);
        }
    }

    void RemoveWeapons()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            client.PlayerObject.GetComponent<WeaponHandler>().UnequipWeaponRpc();
        }
    }
}
