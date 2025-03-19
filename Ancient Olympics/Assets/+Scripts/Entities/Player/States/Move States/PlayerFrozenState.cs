using UnityEngine;

public class PlayerFrozenState : PlayerBaseState
{
    float time;

    public PlayerFrozenState(Player player, PlayerStates states, float time) : base(player, states)
    {
        this.time = time;
    }

    public override void Enter()
    {
        player.CooldownHandler.StartCooldown("Frozen", time); // Apply freeze for duration 'time'
        player.CooldownHandler.onFrozenCooldownCompleted += SwitchState; // Switch state when freeze is over
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        player.CooldownHandler.onFrozenCooldownCompleted -= SwitchState;
    }

    void SwitchState()
    {
        if (player.MoveInput == Vector2.zero)
            SwitchState(states.Idle());
        else
            SwitchState(states.Move());
    }
}
