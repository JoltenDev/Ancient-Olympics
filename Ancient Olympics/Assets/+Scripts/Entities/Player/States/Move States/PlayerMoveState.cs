using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    Vector3 direction;
    Vector3 relativeDirection;

    public PlayerMoveState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        ActionEvent.onAnimatorMove?.Invoke("IsMoving", true); // Start movement animation

        player.InputHandler.onDodgeInput += SwitchState; // Subscribe dodge input to switch to dodge state
        player.InputHandler.onAttackInput += SwitchState;
    }

    public override void Update()
    {
        SwitchState(player.MoveInput); // Switch state to idle when move input is Vector2.zero

        direction = new Vector3(player.MoveInput.x, 0, player.MoveInput.y);
        relativeDirection = player.transform.forward * direction.z + player.transform.right * direction.x;
    }

    public override void FixedUpdate()
    {
        player.SendMove(relativeDirection, player.transform.position); // Send movement to server
    }

    public override void Exit()
    {
        player.InputHandler.onDodgeInput -= SwitchState;
        player.InputHandler.onAttackInput -= SwitchState;

        ActionEvent.onAnimatorMove?.Invoke("IsMoving", false); // Stop move animation
    }

    void SwitchState(Vector2 input)
    {
        if (input == Vector2.zero)
            SwitchState(states.Idle());
    }

    void SwitchState(string input)
    {
        if (input == "Attack")
            SwitchState(states.Melee1());

        if (input == "Dodge")
            SwitchState(states.Dodge());
    }
}
