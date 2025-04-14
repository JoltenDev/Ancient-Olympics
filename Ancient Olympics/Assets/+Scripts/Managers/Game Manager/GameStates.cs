using UnityEngine;

public class GameStates 
{
    GameManager gameManager;

    public GameStates(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public GameBaseState GameLobbyState() { return new GameLobbyState(gameManager, this); }
    public GameBaseState GameTransitionState(bool transitionFromRounds = true) { return new GameTransitionState(gameManager, this, transitionFromRounds); }
    public GameBaseState GameJavelinThrowState() { return new GameJavelinThrowState(gameManager, this); }
    public GameBaseState GameAssassinationState() { return new GameAssassinationState(gameManager, this); }
    public GameBaseState GameJoustState() { return new GameJoustState(gameManager, this); }
    public GameBaseState GameSwordFightState() { return new GameSwordFightState(gameManager, this); }
    public GameBaseState GameEndState() { return new GameEndState(gameManager, this); }
}
