using UnityEngine;

public class PlayerJavelinThrowState : PlayerBaseState
{
    public PlayerJavelinThrowState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        WeaponHandler.onSwingStarted?.Invoke();

        ActionEvent.onAnimatorCrossFade?.Invoke("throw", 0.25f);

        player.WeaponHandler.EquippedWeapon.GetComponent<HomingJavelin>().FindTarget();

        player.CooldownHandler.StartTimer("Attack Duration", .25f);
        player.onAttackDurationCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }
    
    public override void Exit()
    {
        WeaponHandler.onSwingCompleted?.Invoke();
    }

    void SwitchState()
    {
        SwitchState(states.Idle());
    }
}
