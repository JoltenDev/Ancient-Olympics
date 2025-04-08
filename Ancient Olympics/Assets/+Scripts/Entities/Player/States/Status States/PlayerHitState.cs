using Unity.Netcode;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    ulong id = 100;
    float knockback = 0;

    public PlayerHitState(Player player, PlayerStates states, ulong id, float knockback) : base(player, states)
    {
        this.knockback = knockback;
        this.id = id;
    }

    public override void Enter()
    {
        player.onAnimatorCrossFade?.Invoke("hit1", 0.25f); // Start hit animation
        player.CooldownHandler.StartTimer("Hit", 0.5f);

        Transform otherPlayer = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform;
        player.SendPushRpc(otherPlayer.forward, knockback); // Send force to server

        if (player.NetworkHealth.CurrentHealth.Value <= 0)
            SwitchState(states.Death());

        player.onHitCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.onHitCompleted -= SwitchState;
    }

    void SwitchState()
    {
        SwitchState(states.Idle());
    }
}
