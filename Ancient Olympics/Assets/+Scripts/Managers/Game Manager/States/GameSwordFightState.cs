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

        // Start game
        gameManager.onGameStartingCompleted += BeginGame;
    }

    public override void Update()
    {
        if (gameManager.Timer.timerRemainingTimes.ContainsKey("Game Starting"))
        {
            float time = gameManager.Timer.timerRemainingTimes["Game Starting"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"<color=#904bae>Sword Fighting <color=#ffffff>has been chosen!\nStarting... {time:F1}s");
        }
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        gameManager.onGameStartingCompleted -= BeginGame;

        gameManager.AssignWeaponToEveryPlayer(0, true);
    }

    void BeginGame()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Begin!"); // Update Message

        gameManager.AssignWeaponToEveryPlayer(0);
    }
}
