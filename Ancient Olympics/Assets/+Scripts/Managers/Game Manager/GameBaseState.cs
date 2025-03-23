using UnityEngine;

public abstract class GameBaseState
{
    protected GameManager gameManager;
    protected GameStates states;

    public GameBaseState(GameManager gameManager, GameStates states)
    {
        this.gameManager = gameManager;
        this.states = states;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();

    public void SwitchState(GameBaseState newState) 
    {
        Exit(); // Exit current state
        newState.Enter(); // Enter new state

        gameManager.CurrentState = newState;
    }
}
