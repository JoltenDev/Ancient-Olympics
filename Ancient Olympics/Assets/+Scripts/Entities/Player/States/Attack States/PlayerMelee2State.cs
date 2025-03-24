using UnityEngine;

public class PlayerMelee2State : PlayerBaseState
{
    bool canAttack = false;

    public PlayerMelee2State(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        WeaponHandler.onSwingStarted?.Invoke();

        ActionEvent.onAnimatorCrossFade?.Invoke("melee2", 0.25f);
        player.CooldownHandler.StartTimer("Attack Duration", 0.75f);
        player.CooldownHandler.StartTimer("Combo Window", 0.5f);

        player.InputHandler.onAttackInput += SwitchToNextAttack;
        player.onAttackDurationCompleted += ResetState;
        player.onComboWindowCompleted += CanAttack;
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

        player.InputHandler.onAttackInput -= SwitchToNextAttack;
        player.onAttackDurationCompleted -= ResetState;
        player.onComboWindowCompleted -= CanAttack;

        canAttack = false;
    }

    void SwitchToNextAttack(string input)
    {
        if (canAttack)
            SwitchState(states.Melee3());
    }

    void ResetState()
    {
        SwitchState(states.Idle());
    }

    void CanAttack() => canAttack = true;
}
