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

        // Start game
        gameManager.onGameStartingCompleted += BeginGame;
    }

    public override void Update()
    {
        if (gameManager.Timer.timerRemainingTimes.ContainsKey("Game Starting"))
        {
            float time = gameManager.Timer.timerRemainingTimes["Game Starting"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"<color=#4dbfde>Jousting <color=#ffffff>has been chosen!\nStarting... {time:F1}s");
        }
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.onGameStartingCompleted -= BeginGame;

        HandleHorsesForEveryPlayer();
        gameManager.AssignWeaponToEveryPlayer(3, true);
    }

    void BeginGame()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Begin!"); // Update Message

        HandleHorsesForEveryPlayer();
        gameManager.AssignWeaponToEveryPlayer(3);
    }

    void HandleHorsesForEveryPlayer()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var client = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;

            if (!horseActivated)
                client.GetComponent<Player>().ActivateHorseRpc();
            else
                client.GetComponent<Player>().DeactivateHorseRpc("idle");
        }

        horseActivated = !horseActivated;
    }
}
