using System;
using UnityEngine;

public class PlayerInputHandler
{
    Controls blockableControls;
    Controls unblockableControls;
    public event Action<Vector2> onMoveInput;
    public event Action<string> onDodgeInput;
    public event Action<string> onAttackInput;
    public event Action<string> onCatchInput;
    public event Action onEscapeInput;

    public void Initialize()
    {
        if (blockableControls != null) return;

        blockableControls = new Controls();
        unblockableControls = new Controls();

        blockableControls.Player.Move.performed += ctx => onMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        blockableControls.Player.Move.canceled += ctx => onMoveInput?.Invoke(Vector2.zero);

        blockableControls.Player.OnSpace.performed += ctx => onDodgeInput?.Invoke("Dodge");
        blockableControls.Player.OnLeftClick.performed += ctx => onAttackInput?.Invoke("Attack");
        blockableControls.Player.OnRightClick.performed += ctx => onCatchInput?.Invoke("Catch");

        unblockableControls.Player.OnEscape.performed += ctx => onEscapeInput?.Invoke();

        blockableControls.Enable();
        unblockableControls.Enable();
    }

    public void UnblockInput() => blockableControls?.Enable();
    public void BlockInput() => blockableControls?.Disable();

    public void Dispose()
    {
        if (blockableControls == null) return;
        if (unblockableControls == null) return;

        blockableControls.Disable();
        unblockableControls.Dispose();
    }
}