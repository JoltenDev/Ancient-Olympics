using UnityEngine;

public class PlayerStates {
    Player player;

    public PlayerStates(Player currentPlayer)
    {
        player = currentPlayer;
    }

    public PlayerBaseState Idle() { return new PlayerIdleState(player, this); }
    public PlayerBaseState Move() { return new PlayerMoveState(player, this); }
    public PlayerBaseState Dodge() { return new PlayerDodgeState(player, this); }
    public PlayerBaseState Frozen(float time) { return new PlayerFrozenState(player, this, time); }
    public PlayerBaseState Melee1() { return new PlayerMelee1State(player, this); }
    public PlayerBaseState Melee2() { return new PlayerMelee2State(player, this); }
    public PlayerBaseState Melee3() { return new PlayerMelee3State(player, this); }
    public PlayerBaseState Hit() { return new PlayerHitState(player, this); }
    public PlayerBaseState Death() { return new PlayerDeathState(player, this); }
}
