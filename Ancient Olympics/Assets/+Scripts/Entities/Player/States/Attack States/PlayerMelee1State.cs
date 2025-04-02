using UnityEngine;

public class PlayerMelee1State : PlayerBaseState
{
    bool canAttack = false;

    public PlayerMelee1State(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.WeaponHandler.onSwingStarted?.Invoke();

        player.onAnimatorCrossFade?.Invoke("melee1", 0.25f);
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
        player.WeaponHandler.onSwingCompleted?.Invoke();

        player.InputHandler.onAttackInput -= SwitchToNextAttack;
        player.onAttackDurationCompleted -= ResetState;
        player.onComboWindowCompleted -= CanAttack;

        canAttack = false;
    }

    void SwitchToNextAttack(string input)
    {
        if (canAttack)
            SwitchState(states.Melee2());
    }

    void ResetState()
    {
        SwitchState(states.Idle());
    }

    void CanAttack() => canAttack = true;
}
