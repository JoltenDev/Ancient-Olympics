using Unity.Netcode;
using UnityEngine;

public class GameAssassinationState : GameBaseState
{
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
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.onGameStartingCompleted -= BeginGame;

        gameManager.AssignWeaponToEveryPlayer(1, true);
        HideUsernames(false);
        gameManager.DespawnAllNpcsRpc();
    }

    void BeginGame()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Begin!"); // Update Message

        gameManager.AssignWeaponToEveryPlayer(1, false, true);
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
}
