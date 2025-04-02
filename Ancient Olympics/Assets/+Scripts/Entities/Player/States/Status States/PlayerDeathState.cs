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
        SendDeath(player.OwnerClientId);

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
        player.onAnimatorCrossFade?.Invoke("idle", 0.25f);
    }

    void SwitchState()
    {
    }

    
    void SendDeath(ulong id)
    {
        GameManager.Instance?.AddPlayerToDeadListRpc(id);
    }
}
