using UnityEngine;

public class PlayerDeathState : PlayerBaseState
{
    public PlayerDeathState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        ActionEvent.onAnimatorCrossFade?.Invoke("death", 0.25f); // Start Death animation
        ActionEvent.onDeath?.Invoke();

        player.InputHandler.BlockInput();
        player.DisablePlayerRpc();
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
