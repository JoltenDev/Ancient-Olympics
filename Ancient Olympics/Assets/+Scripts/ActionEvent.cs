using System;
using UnityEngine;

public static class ActionEvent
{
    // Game Actions
    public static Action onStateChanged;

    // Player Actions
    public static Action<float> onHealthChanged;
    public static Action onSwingStarted;
    public static Action onSwingCompleted;
    public static Action<float> onDamage;
    public static Action<float> onHeal;

    // Animator Actions
    public static Action<string, bool> onAnimatorMove;
    public static Action<string, float> onAnimatorDodge;
    public static Action<string, float> onAnimatorMelee;
    public static Action<string, float> onAnimatorHit;
}
