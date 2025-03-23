using UnityEngine;

public class GameStates 
{
    GameManager gameManager;

    public GameStates(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public GameBaseState GameLobbyState() { return new GameLobbyState(gameManager, this); }
    public GameBaseState GameTransitionState() { return new GameTransitionState(gameManager, this); }
}
