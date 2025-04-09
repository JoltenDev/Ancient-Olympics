using UnityEngine;

public class PlayerMelee3State : PlayerBaseState
{
    public PlayerMelee3State(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.WeaponHandler.onSwingStarted?.Invoke();

        player.onAnimatorCrossFade?.Invoke("melee3", 0.25f);
        player.CooldownHandler.StartTimer("Attack Duration", 0.75f);

        player.SendPushRpc(player.MoveInput, player.transform.forward, player.transform.right, 3f); // Send force to server
        player.SendPushRpc(player.NetworkHealth.VictimId, player.transform.forward, 2f);

        player.onAttackDurationCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        player.WeaponHandler.onSwingCompleted?.Invoke();
        player.onAttackDurationCompleted -= SwitchState;
    }

    void SwitchState()
    {
        SwitchState(states.Frozen(.25f));
    }
}
