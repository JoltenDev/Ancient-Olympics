using UnityEditor.Build;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.InputHandler.onAttackInput += SwitchState;
        player.InputHandler.onCatchInput += SwitchState;
    }

    public override void Update()
    {
        SwitchState(player.MoveInput); // Switch to move state when input is not Vector2.zero
    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.InputHandler.onAttackInput -= SwitchState;
        player.InputHandler.onCatchInput -= SwitchState;
    }

    void SwitchState(Vector2 input)
    {
        if (input != Vector2.zero)
            SwitchState(states.Move());
    }

    void SwitchState(string input)
    {
        if (input == "Attack")
        {
            switch (player.WeaponHandler.WeaponData?.attackType)
            {
                case AttackType.MeleeSword:
                    SwitchState(states.Melee1());
                    break;
                case AttackType.Throw:
                    SwitchState(states.JavelinThrow());
                    break;
            }
        }

        if (input == "Catch")
            SwitchState(states.Catch());
    }
}
