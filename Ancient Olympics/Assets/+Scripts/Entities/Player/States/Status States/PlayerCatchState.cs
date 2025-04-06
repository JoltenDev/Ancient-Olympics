using Unity.Netcode;
using UnityEngine;

public class PlayerCatchState : PlayerBaseState
{
    public PlayerCatchState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.onAnimatorCrossFade?.Invoke("catch", 0.25f);
        player.NetworkHealth.SetImmunityRpc(player.OwnerClientId, true);

        player.CooldownHandler.StartTimer("Attack Duration", .75f); // Catch for .75 seconds
        player.onAttackDurationCompleted += Freeze;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }
    
    public override void Exit()
    {
        player.onAttackDurationCompleted -= Freeze;
        player.NetworkHealth.SetImmunityRpc(player.OwnerClientId, false);
    }

    void Freeze()
    {
        SwitchState(states.Frozen(1f)); // Freeze for a second
    }
}
