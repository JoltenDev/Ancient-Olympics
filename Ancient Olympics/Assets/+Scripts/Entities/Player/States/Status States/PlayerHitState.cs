using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    public PlayerHitState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.onAnimatorCrossFade?.Invoke("hit1", 0.25f); // Start hit animation

        player.CooldownHandler.StartTimer("Hit", 0.5f);

        if (player.NetworkHealth.CurrentHealth.Value <= 0)
            SwitchState(states.Death());

        player.onHitCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.onHitCompleted -= SwitchState;
    }

    void SwitchState()
    {
        SwitchState(states.Idle());
    }
}
