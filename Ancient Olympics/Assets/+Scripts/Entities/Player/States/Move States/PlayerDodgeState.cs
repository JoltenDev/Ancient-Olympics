using UnityEngine;

public class PlayerDodgeState : PlayerBaseState
{
    public PlayerDodgeState(Player player, PlayerStates states) : base(player, states)
    {

    }

    public override void Enter()
    {
        player.onAnimatorCrossFade?.Invoke("dodge", .25f); // Invoke dodge animation

        player.SendPushRpc(player.MoveInput, player.transform.forward, player.transform.right, 10f); // Send force to server

        player.CooldownHandler.StartTimer("Dodge", 1.5f); // Start dodge cooldown
        SwitchState(states.Frozen(1f)); // Freeze for a second after dodging
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
