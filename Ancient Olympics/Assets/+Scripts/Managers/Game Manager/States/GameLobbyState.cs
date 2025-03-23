using System;
using UnityEngine;

public class GameLobbyState : GameBaseState
{
    public GameLobbyState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        // Load the lobby scene and instantiate the lobby menu
        NetworkSceneManager.Instance.ChangeScene("Scene_Lobby", () =>
        {
            if (gameManager.IsServer)
            {
                ActionEvent.onGameStarted += gameManager.RepositionPlayersRpc;
            }
        });
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
    }

    void SwitchState()
    {
    }
}
