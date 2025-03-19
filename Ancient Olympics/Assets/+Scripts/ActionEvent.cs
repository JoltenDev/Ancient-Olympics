using System;
using UnityEngine;

public static class ActionEvent
{
    // Game Actions
    public static Action onStateChanged;

    public static Action<float> onHealthChanged;
    public static Action onAttack;
    public static Action<float> onDamage;
    public static Action<float> onHeal;

    // Animator Actions
    public static Action<string, bool> onAnimatorMove;
    public static Action<string, float> onAnimatorDodge;
    public static Action<string, float> onAnimatorMelee;
}
