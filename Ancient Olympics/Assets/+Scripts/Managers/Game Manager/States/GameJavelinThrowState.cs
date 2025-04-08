using Unity.Netcode;
using UnityEngine;

public class GameJavelinThrowState : GameBaseState
{
    public GameJavelinThrowState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StartTimer("Game Starting", 10);
        gameManager.startMessage = "<color=#7cd145>Javelin Throwing";

        gameManager.SetJavelinAddendRpc(0);

        // Start game
        gameManager.onGameStartingCompleted += BeginGame;
        gameManager.DeadPlayers.OnListChanged += PlayerDied;
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

        gameManager.SetJavelinAddendRpc(0);

        gameManager.DeadPlayers.OnListChanged -= PlayerDied;
        gameManager.onGameStartingCompleted -= BeginGame;
    }

    void BeginGame()
    {
        gameManager.Timer.StartTimer("Round Active", 25);

        var client = RandomClient();
        client.PlayerObject.GetComponent<WeaponHandler>().EquipWeaponRpc(client.ClientId, 2);
    }

    void PlayerDied(NetworkListEvent<ulong> changeEvent)
    {
        gameManager.SetJavelinAddendRpc(0);
    }

    NetworkClient RandomClient()
    {
        return NetworkManager.Singleton.ConnectedClients[
            (ulong) UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClients.Count)];
    }
}
