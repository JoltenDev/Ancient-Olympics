using Unity.Netcode;
using UnityEngine;

public class GameSwordFightState : GameBaseState
{
    public GameSwordFightState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StartTimer("Game Starting", 10);
        gameManager.startMessage = "<color=#904bae>Sword Fighting";

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

        gameManager.AssignWeaponToEveryPlayer(0, true);

        gameManager.onGameStartingCompleted -= BeginGame;
    }

    void BeginGame()
    {
        gameManager.Timer.StartTimer("Round Active", 300);

        gameManager.AssignWeaponToEveryPlayer(0);
    }
}
