using System;
using UnityEngine;

public static class ActionEvent
{
    // Game Actions
    public static Action onGameStarted;

    // Player Actions
    public static Action<float> onHealthChanged;
}
