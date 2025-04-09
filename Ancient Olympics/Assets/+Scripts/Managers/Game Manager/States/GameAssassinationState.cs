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
        gameManager.startMessage = "<color=#ff6666>Assassination";

        gameManager.SpawnNpcsRpc(15);
        HideUsernames();

        // Start game
        gameManager.onGameStartingCompleted += BeginGame;
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.startMessage = "";

        gameManager.AssignWeaponToEveryPlayer(1, true);
        HideUsernames(false);
        gameManager.DespawnAllNpcsRpc();

        gameManager.onGameStartingCompleted -= BeginGame;
    }

    void BeginGame()
    {
        gameManager.Timer.StartTimer("Round Active", 300);
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
