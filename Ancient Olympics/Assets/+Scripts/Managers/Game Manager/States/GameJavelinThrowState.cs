using System;
using UnityEngine;

public class GameJavelinThrowState : GameBaseState
{
    public GameJavelinThrowState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StartTimer("GameStarting", 10);

        // Start game
        gameManager.onGameStartingCompleted += UpdateMessage;
    }

    public override void Update()
    {
        if (gameManager.Timer.timerRemainingTimes.ContainsKey("GameStarting"))
        {
            float time = gameManager.Timer.timerRemainingTimes["GameStarting"];

            if (time > 0)
                UIManager.Instance.UpdatePlayerTimersRpc($"<color=#ff6666>Javelin Throwing <color=#ffffff>has been chosen!\nStarting... {time:F1}s");
        }
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
    }

    void UpdateMessage()
    {
        UIManager.Instance.UpdatePlayerTimersRpc($"Begin!");
    }

    void SwitchState()
    {
    }
}
