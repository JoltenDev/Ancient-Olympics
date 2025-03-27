using Unity.Netcode;
using UnityEngine;

public class PlayerDeathState : PlayerBaseState
{
    public PlayerDeathState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        ActionEvent.onAnimatorCrossFade?.Invoke("death", 0.25f); // Start Death animation
        SendDeath(player.OwnerClientId);

        player.InputHandler.BlockInput();
        player.DisablePlayerRpc();

        Cursor.SetCursor(player.DeathTexture, Vector2.zero, CursorMode.Auto);
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        Cursor.SetCursor(player.DefaultTexture, Vector2.zero, CursorMode.Auto);
    }

    void SwitchState()
    {
    }

    [Rpc(SendTo.Server)]
    void SendDeath(ulong id)
    {
        if (!player.IsServer) return;
        
        GameManager.Instance.DeadPlayers?.Add(id);
    }
}
