using Unity.Netcode;
using UnityEngine;

public class GameJavelinThrowState : GameBaseState
{
    float savedTime = Time.time;
    float tick = 3f;

    bool gameStarted = false;

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
        if (gameManager.CurrentJavelinWielder.Value != 100)
        {
            var id = gameManager.CurrentJavelinWielder.Value;
            if (Time.time > savedTime && gameStarted)
            {
                NetworkManager.Singleton.ConnectedClients[id]
                    .PlayerObject.GetComponent<NetworkHealth>().SendDamageRpc(5, id, id);

                savedTime = Time.time + tick;
            }
        }
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.startMessage = "";

        gameManager.SetJavelinAddendRpc(0);
        gameManager.SetJavelinWielderRpc(100);

        gameManager.AssignWeaponToEveryPlayer(2, true);

        gameManager.DeadPlayers.OnListChanged -= PlayerDied;
        gameManager.onGameStartingCompleted -= BeginGame;
    }

    void BeginGame()
    {
        gameManager.Timer.StartTimer("Round Active", 300);

        gameStarted = true;

        var client = RandomClient();
        client.PlayerObject.GetComponent<WeaponHandler>().EquipWeaponRpc(client.ClientId, 2);
        gameManager.SetJavelinWielderRpc(client.ClientId);
    }

    void PlayerDied()
    {
        gameManager.SetJavelinAddendRpc(0);
    }

    NetworkClient RandomClient()
    {
        return NetworkManager.Singleton.ConnectedClients[
            (ulong) UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClients.Count)];
    }
}
