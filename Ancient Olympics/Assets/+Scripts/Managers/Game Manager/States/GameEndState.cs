using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameEndState : GameBaseState
{
    ulong winnerId = 0;
    ulong secondId = 0;
    ulong thirdId = 0;

    public GameEndState(GameManager gameManager, GameStates states) : base(gameManager, states)
    {
    }

    public override void Enter()
    {
        gameManager.Timer.StopTimer("Round Active");

        UIManager.Instance.HealthUIRpc(false);
        UIManager.Instance.DisplayLeaveUIRpc();

        TeleportPlayers();

        gameManager.Timer.StartTimer("Game Ending", 3);

        gameManager.onGameEndingCompleted += ShowWinner;
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        UIManager.Instance.GlobalMessageRpc(false);

        gameManager.ResetGamemodes();
        gameManager.gameEnded = false;
        gameManager.RoundWins.Clear();

        gameManager.onGameEndingCompleted -= ShowWinner;
    }

    void ShowWinner()
    {
        string winnerName = NetworkManager.Singleton.ConnectedClients[winnerId].PlayerObject.GetComponent<Player>().Username;
        UIManager.Instance.UpdatePlayerTimersRpc($"<color=#ffffff>{winnerName}<color=#2E2E2E>!");
    }

    void BlockEveryPlayerInput(bool block = true)
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var player = NetworkLobbyManager.Instance.NetworkManager.ConnectedClients[id].PlayerObject;
            if (block)
                player.GetComponent<Player>().InputHandler.BlockInput();
            else
                player.GetComponent<Player>().InputHandler.UnblockInput();
        }
    }

    void TeleportPlayers()
    {
        var sortedPlayers = gameManager.RoundWins
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();

        if (sortedPlayers.Count > 0)
        {
            winnerId = sortedPlayers[0];
            gameManager.RepositionPlayerRpc(winnerId, new Vector3(0, 0.9f, 1.3f));
        }

        if (sortedPlayers.Count > 1)
        {
            secondId = sortedPlayers[1];
            gameManager.RepositionPlayerRpc(secondId, new Vector3(-1.15f, 0.65f, 0.65f));
        }

        if (sortedPlayers.Count > 2)
        {
            thirdId = sortedPlayers[2];
            gameManager.RepositionPlayerRpc(thirdId, new Vector3(1.2f, 0.55f, 0.7f));
        }

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var id = sortedPlayers[i];

            if (id != winnerId || id != secondId || id != thirdId)
                gameManager.RepositionPlayerRpc(id, new Vector3(0, 0.4f, 0.45f));
        }
    }
}
