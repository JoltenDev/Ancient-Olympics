using UnityEngine;

public class PlayerMelee3State : PlayerBaseState
{
    public PlayerMelee3State(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        ActionEvent.onSwingStarted?.Invoke();

        ActionEvent.onAnimatorCrossFade?.Invoke("melee3", 0.25f);
        player.CooldownHandler.StartCooldown("Attack Duration", 0.75f);

        player.CooldownHandler.onAttackDurationCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        ActionEvent.onSwingCompleted?.Invoke();
        player.CooldownHandler.onAttackDurationCompleted -= SwitchState;
    }

    void SwitchState()
    {
        SwitchState(states.Frozen(.25f));
    }
}
