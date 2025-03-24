using UnityEngine;
using Unity.Netcode;

public class GameTransitionState : GameBaseState
{
    enum Modes { Jousting, Assassination, SwordFighting, JavelinThrowing}

    public GameTransitionState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        // Load the arena scene and handle other game state transitions
        NetworkSceneManager.Instance.ChangeScene("Scene_Arena", () =>
        {
            ActionEvent.onGameStarted?.Invoke();
            UIManager.Instance.ActivateHudUIRpc(); // Activate player's hud
        });

        UIManager.Instance.ActivateGlobalMessageRpc(); // Activate global message ui

        gameManager.Timer.StartTimer("Transition", 5); // Start timer til gamemode starts
        gameManager.onTransitionCompleted += SelectGameMode;
    }

    public override void Update()
    {
        if (gameManager.Timer.timerRemainingTimes.ContainsKey("Transition"))
        {
            float time = gameManager.Timer.timerRemainingTimes["Transition"];
            UIManager.Instance.UpdatePlayerTimersRpc($"Selecting Game... {time:F1}s");
        }
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
    }

    void SelectGameMode()
    {
        var mode = (Modes) Random.Range(3, 3);

        switch (mode)
        {
            case Modes.JavelinThrowing:
                SwitchState(states.GameJavelinThrowState());
                break;
        }
    }
}
