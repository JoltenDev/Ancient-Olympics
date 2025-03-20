using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    public PlayerHitState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        ActionEvent.onAnimatorHit?.Invoke("hit1", 0.25f); // Start hit animation

        player.CooldownHandler.StartCooldown("Hit", 0.5f);
        player.CooldownHandler.onHitCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.CooldownHandler.onHitCompleted -= SwitchState;
    }

    void SwitchState()
    {
        SwitchState(states.Idle());
    }
}
