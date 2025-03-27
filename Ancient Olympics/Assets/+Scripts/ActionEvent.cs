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

    // Animator Actions
    public static Action<string, bool> onAnimatorSetBool;
    public static Action<string, float> onAnimatorCrossFade;
}
