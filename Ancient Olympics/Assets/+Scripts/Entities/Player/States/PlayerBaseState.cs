using UnityEngine;

public abstract class PlayerBaseState
{
    protected Player player;
    protected PlayerStates states;

    public PlayerBaseState(Player player, PlayerStates states)
    {
        this.player = player;
        this.states = states;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();

    void UpdateState() { }
    public void SwitchState(PlayerBaseState newState) 
    {
        Exit(); // Exit current state
        newState.Enter(); // Enter new state

        player.CurrentState = newState;
    }
}
