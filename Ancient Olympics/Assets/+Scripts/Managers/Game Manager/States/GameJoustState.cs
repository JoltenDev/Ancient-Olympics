using Unity.Netcode;
using UnityEngine;

public class GameJoustState : GameBaseState
{
    bool horseActivated = false;

    public GameJoustState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StartTimer("Game Starting", 10);
        gameManager.startMessage = "<color=#4dbfde>Jousting";

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

        HandleHorsesForEveryPlayer();
        gameManager.AssignWeaponToEveryPlayer(3, true);

        gameManager.onGameStartingCompleted -= BeginGame;
    }

    void BeginGame()
    {
        gameManager.Timer.StartTimer("Round Active", 300);

        HandleHorsesForEveryPlayer();
        gameManager.AssignWeaponToEveryPlayer(3);
    }

    void HandleHorsesForEveryPlayer()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var client = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;

            if (client.GetComponent<NetworkHealth>().Dead.Value) continue;

            if (!horseActivated)
                client.GetComponent<Player>().ActivateHorseRpc();
            else
                client.GetComponent<Player>().DeactivateHorseRpc();
        }

        horseActivated = !horseActivated;
    }
}
