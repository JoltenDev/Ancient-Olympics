using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTransitionState : GameBaseState
{
    enum Modes { JavelinThrowing, Assassination, Jousting, SwordFighting }
    bool transitionFromRound;

    public GameTransitionState(GameManager gameManager, GameStates states, bool transitionFromRound = true) : base(gameManager, states)
    {
        this.transitionFromRound = transitionFromRound;
    }

    public override void Enter()
    {
        LoadArenaScene();

        UIManager.Instance.GlobalMessageRpc(true); // Activate global message ui

        Transition();
    }

    public override void Update()
    {
        if (!transitionFromRound && gameManager.Timer.timerRemainingTimes.ContainsKey("Transition"))
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
        gameManager.onTransitionCompleted -= SelectGameMode;
        gameManager.RepositionPlayersRpc();
    }

    void LoadArenaScene()
    {
        if (SceneManager.GetActiveScene().name != "Scene_Arena")
        {
            // Load the arena scene and handle other game state transitions
            NetworkSceneManager.Instance.ChangeScene("Scene_Arena", () =>
            {
                ActionEvent.onGameStarted?.Invoke();
                UIManager.Instance.HealthUIRpc(true);
            });
        }
    }

    void Transition()
    {
        if (!transitionFromRound)
        {
            gameManager.Timer.StartTimer("Transition", 5); // Start timer til gamemode starts
            gameManager.onTransitionCompleted += SelectGameMode;
        }
        else
        {
            gameManager.Timer.StopTimer("Round Active"); // Stop the round timer;

            gameManager.Timer.StartTimer("Buffer", 5); // Start buffer timer (Buffer -> Transition)
            gameManager.onBufferCompleted += StartTransition;

            void StartTransition()
            {
                if (GameManager.Instance.DeadPlayers.Count > 0)
                    GameManager.Instance.RevivePlayersRpc(); // Revive all players for next round

                if (GameManager.Instance.IsServer)
                    GameManager.Instance.DeadPlayers.Clear(); // Clear dead players

                gameManager.Timer.StartTimer("Transition", 5); // Start timer til gamemode starts (Transition -> Selection)
                gameManager.onTransitionCompleted += SelectGameMode;
                transitionFromRound = false;

                gameManager.onBufferCompleted -= StartTransition;
            }
        }
    }

    void SelectGameMode()
    {
        var mode = (Modes) Random.Range(0, 4);

        switch (mode)
        {
            case Modes.JavelinThrowing:
                SwitchState(states.GameJavelinThrowState());
                break;
            case Modes.Assassination:
                SwitchState(states.GameAssassinationState());
                break;
            case Modes.Jousting:
                SwitchState(states.GameJoustState());
                break;
            case Modes.SwordFighting:
                SwitchState(states.GameSwordFightState());
                break;
        }
    }
}
