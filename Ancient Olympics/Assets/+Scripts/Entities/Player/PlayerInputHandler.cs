using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputHandler
{
    Controls controls;
    public event Action<Vector2> onMoveInput;
    public event Action<string> onDodgeInput;
    public event Action<string> onAttackInput;

    public void Initialize()
    {
        if (controls != null) return;

        controls = new Controls();

        controls.Player.Move.performed += ctx => onMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => onMoveInput?.Invoke(Vector2.zero);

        controls.Player.OnSpace.performed += ctx => onDodgeInput?.Invoke("Dodge");
        controls.Player.OnLeftClick.performed += ctx => onAttackInput?.Invoke("Attack");

        controls.Enable();
    }

    public void Dispose()
    {
        if (controls == null) return;

        controls.Disable();
    }
}