using UnityEngine;

public class PlayerMelee1State : PlayerBaseState
{
    bool canAttack = false;

    public PlayerMelee1State(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        ActionEvent.onAttack?.Invoke();

        ActionEvent.onAnimatorMelee?.Invoke("melee1", 0.25f);
        player.CooldownHandler.StartCooldown("Attack Duration", 0.75f);
        player.CooldownHandler.StartCooldown("Combo Window", 0.5f);

        player.InputHandler.onAttackInput += SwitchToNextAttack;
        player.CooldownHandler.onAttackDurationCompleted += ResetState;
        player.CooldownHandler.onComboWindowCompleted += CanAttack;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.InputHandler.onAttackInput -= SwitchToNextAttack;
        player.CooldownHandler.onAttackDurationCompleted -= ResetState;
        player.CooldownHandler.onComboWindowCompleted -= CanAttack;

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
