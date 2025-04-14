using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTransitionState : GameBaseState
{
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
        gameManager.onTransitionCompleted -= gameManager.SwitchRandomGamemode;
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
            gameManager.onTransitionCompleted += gameManager.SwitchRandomGamemode;
        }
        else
        {
            gameManager.Timer.StopTimer("Round Active"); // Stop the round timer;

            gameManager.Timer.StartTimer("Buffer", 5); // Start buffer timer (Buffer -> Transition)
            gameManager.onBufferCompleted += StartTransition;

            void StartTransition()
            {
                if (gameManager.DeadPlayers.Count() > 0)
                {
                    gameManager.RevivePlayersRpc();
                    gameManager.DeadPlayers.Clear(); // Clear dead players
                }
                if (!gameManager.gameEnded)
                {
                    gameManager.Timer.StartTimer("Transition", 5); // Start timer til gamemode starts (Transition -> Selection)
                    gameManager.onTransitionCompleted += gameManager.SwitchRandomGamemode;
                    transitionFromRound = false;
                }
                else
                {
                    NetworkSceneManager.Instance.ChangeScene("Scene_Win");
                    SwitchState(states.GameEndState());
                }

                gameManager.onBufferCompleted -= StartTransition;
            }
        }
    }
}
