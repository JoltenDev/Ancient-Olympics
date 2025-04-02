using UnityEngine;

public class PlayerSwingState : PlayerBaseState
{
    public PlayerSwingState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.WeaponHandler.onSwingStarted?.Invoke();

        player.onAnimatorCrossFade?.Invoke("melee1", 0.25f);
        player.CooldownHandler.StartTimer("Attack Duration", 0.75f);

        player.onAttackDurationCompleted += Freeze;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.WeaponHandler.onSwingCompleted?.Invoke();
        player.onAttackDurationCompleted -= Freeze;
    }

    void Freeze()
    {
        SwitchState(states.Frozen(1f));
    }
}
