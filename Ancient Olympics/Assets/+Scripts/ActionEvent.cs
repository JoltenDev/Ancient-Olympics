using System;
using UnityEngine;

public static class ActionEvent
{
    // Game Actions
    public static Action onGameStarted;
    public static Action onMatchBegan;
    public static Action onMatchEnded;

    // Player Actions
    public static Action<float> onHealthChanged;
    public static Action onSwingStarted;
    public static Action onSwingCompleted;
    public static Action<float> onDamage;
    public static Action<float> onHeal;
    public static Action onDeath;

    // Animator Actions
    public static Action<string, bool> onAnimatorSetBool;
    public static Action<string, float> onAnimatorCrossFade;
}
