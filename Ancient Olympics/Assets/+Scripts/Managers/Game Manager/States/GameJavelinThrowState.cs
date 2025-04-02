using Unity.Netcode;
using UnityEngine;

public class GameJavelinThrowState : GameBaseState
{
    ulong alivePlayerId = 0;

    public GameJavelinThrowState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StartTimer("Game Starting", 10);

        // Start game
        gameManager.onGameStartingCompleted += BeginGame;
    }

    public override void Update()
    {
        if (gameManager.Timer.timerRemainingTimes.ContainsKey("Game Starting"))
        {
            float time = gameManager.Timer.timerRemainingTimes["Game Starting"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"<color=#ff6666>Javelin Throwing <color=#ffffff>has been chosen!\nStarting... {time:F1}s");
        }

        SwitchState(); // Switch state if only 1 player is alive
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.onGameStartingCompleted -= BeginGame;

        Player player = NetworkManager.Singleton.ConnectedClients[alivePlayerId].PlayerObject.GetComponentInChildren<Player>();
        UIManager.Instance.UpdatePlayerTimersRpc($"<color=#59dac9>{player.Username} <color=#ffffff>has won the round!"); // Update Message
    }

    void BeginGame()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Begin!"); // Update Message

        RandomClient().PlayerObject.GetComponent<WeaponHandler>().EquipWeaponClientRpc(2);
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

    NetworkClient RandomClient()
    {
        return NetworkManager.Singleton.ConnectedClients[
            (ulong) UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClients.Count)];
    }
}
