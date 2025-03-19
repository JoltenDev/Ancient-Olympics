using System;
using UnityEngine;

public static class ActionEvent
{
    // Game Actions
    public static Action onStateChanged;

    // Player Actions
    public static Action<float> onFrozenStarted; // Invokes on dodge started, subscribed to frozen state
    public static Action onFrozenCompleted; // Invokes on frozen timer completed, subscribed to idle state

    public static Action onDodgeStarted; // Invokes on dodge button, subscribed to frozen and dodge state
    public static Action onDodgeCompleted; // Invokes on dodge timer completed, subscribed to canDodge bool

    public static Action<float> onStartAttackCooldown; //
    public static Action onAttackCooldownCompleted; //

    public static Action<float> onHealthChanged; //
    public static Action<float> onDamage; //
    public static Action<float> onHeal; //

    // Animator Actions
    public static Action<string, bool> onAnimatorMove;
    public static Action<string, float> onAnimatorDodge;
    public static Action<string, float> onAnimatorMelee;
}
