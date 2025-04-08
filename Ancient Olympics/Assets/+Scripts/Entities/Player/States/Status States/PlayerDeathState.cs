using Unity.Netcode;
using UnityEngine;

public class PlayerDeathState : PlayerBaseState
{
    public PlayerDeathState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.onAnimatorCrossFade?.Invoke("death", 0.25f); // Start Death animation
        Cursor.SetCursor(player.DeathTexture, Vector2.zero, CursorMode.Auto);

        GameManager.Instance?.AddPlayerToDeadListRpc(player.OwnerClientId);

        player.InputHandler.BlockInput();
        player.NetworkHealth.SetDeadRpc(player.OwnerClientId, true);
        player.NetworkHealth.CurrentHealth.OnValueChanged += SwitchState;

        if (player.transform.GetComponent<Horse>().enabled)
        {
            player.transform.GetComponent<Horse>().enabled = false;
            player.DeactivateHorseRpc("death", true);
            player.WeaponHandler.UnequipWeaponRpc();
        }
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.onAnimatorCrossFade?.Invoke("idle", 0.25f);
        Cursor.SetCursor(player.DefaultTexture, Vector2.zero, CursorMode.Auto);

        player.InputHandler.UnblockInput();
        player.NetworkHealth.SetDeadRpc(player.OwnerClientId, false);
        player.NetworkHealth.CurrentHealth.OnValueChanged -= SwitchState;
    }

    void SwitchState(float prevAmount, float newAmount)
    {
        if (player.NetworkHealth.CurrentHealth.Value > 0)
            SwitchState(states.Idle());
    }
}
