using UnityEngine;

public class PlayerStates
{
    Player player;

    public PlayerStates(Player currentPlayer)
    {
        player = currentPlayer;
    }

    public PlayerBaseState Idle() => new PlayerIdleState(player, this);
    public PlayerBaseState Move() => new PlayerMoveState(player, this);
    public PlayerBaseState Dodge() => new PlayerDodgeState(player, this);
    public PlayerBaseState Catch() => new PlayerCatchState(player, this);
    public PlayerBaseState JavelinThrow() => new PlayerJavelinThrowState(player, this);
    public PlayerBaseState Hit(ulong id, float knockback) => new PlayerHitState(player, this, id, knockback);
    public PlayerBaseState Death() => new PlayerDeathState(player, this);
    public PlayerBaseState Frozen(float time) => new PlayerFrozenState(player, this, time);
    public PlayerBaseState Swing() => new PlayerSwingState(player, this);
    public PlayerBaseState Melee1() => new PlayerMelee1State(player, this);
    public PlayerBaseState Melee2() => new PlayerMelee2State(player, this);
    public PlayerBaseState Melee3() => new PlayerMelee3State(player, this);
}
