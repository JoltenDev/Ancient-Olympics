using UnityEngine;

public class PlayerDodgeState : PlayerBaseState
{
    public PlayerDodgeState(Player player, PlayerStates states) : base(player, states)
    {

    }

    public override void Enter()
    {
        ActionEvent.onAnimatorCrossFade?.Invoke("dodge", .25f); // Invoke dodge animation

        player.SendDodgeRpc(player.MoveInput, player.transform.forward, player.transform.right); // Send dodge to server

        player.CooldownHandler.StartCooldown("Dodge", 1.5f); // Start dodge cooldown
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
