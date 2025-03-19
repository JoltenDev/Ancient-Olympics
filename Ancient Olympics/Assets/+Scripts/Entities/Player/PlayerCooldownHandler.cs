using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerCooldownHandler : NetworkBehaviour
{
    private Dictionary<string, Action> cooldownStartedEvents = new Dictionary<string, Action>();
    private Dictionary<string, Action> cooldownCompletedEvents = new Dictionary<string, Action>();
    private Dictionary<string, CancellationTokenSource> activeCooldowns = new Dictionary<string, CancellationTokenSource>();

    public Action onDodgeCooldownStarted;
    public Action onFrozenCooldownStarted;
    public Action onAttackDurationStarted;
    public Action onComboWindowStarted;

    public Action onDodgeCooldownCompleted;
    public Action onFrozenCooldownCompleted;
    public Action onAttackDurationCompleted;
    public Action onComboWindowCompleted;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // Register cooldowns
        RegisterCooldown(cooldownStartedEvents, "Dodge", () => onDodgeCooldownStarted?.Invoke());
        RegisterCooldown(cooldownStartedEvents, "Frozen", () => onFrozenCooldownStarted?.Invoke());
        RegisterCooldown(cooldownStartedEvents, "Attack Duration", () => onAttackDurationStarted?.Invoke());
        RegisterCooldown(cooldownStartedEvents, "Combo Window", () => onComboWindowStarted?.Invoke());

        // Register cooldowns
        RegisterCooldown(cooldownCompletedEvents, "Dodge", () => onDodgeCooldownCompleted?.Invoke());
        RegisterCooldown(cooldownCompletedEvents,"Frozen", () => onFrozenCooldownCompleted?.Invoke());
        RegisterCooldown(cooldownCompletedEvents, "Attack Duration", () => onAttackDurationCompleted?.Invoke());
        RegisterCooldown(cooldownCompletedEvents, "Combo Window", () => onComboWindowCompleted?.Invoke());
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        cooldownStartedEvents.Clear();
        cooldownCompletedEvents.Clear();
    }

    /// <summary>
    /// Registers a new cooldown type with an associated completion event.
    /// </summary>
    /// <param name="dict"> The dictionary storing cooldown events. </param>
    /// <param name="cooldownName"> The unique name of the cooldown to register. </param>
    /// <param name="onCooldownComplete"> The action to invoke when the cooldown completes. </param>
    void RegisterCooldown(Dictionary<string, Action> dict, string cooldownName, Action onCooldownComplete)
    {
        if (!dict.ContainsKey(cooldownName))
        {
            dict[cooldownName] = onCooldownComplete;
        }
    }

    /// <summary>
    /// Starts a cooldown timer asynchronously. If a cooldown with the same name is already running, it cancels the existing one before starting a new timer.
    /// </summary>
    /// <param name="cooldownName"> The name of the cooldown to start. </param>
    /// <param name="duration"> The duration of the cooldown in seconds. </param>
    public void StartCooldown(string cooldownName, float duration)
    {
        if (!cooldownCompletedEvents.ContainsKey(cooldownName)) return;

        // Cancel any existing cooldown with the same name
        if (activeCooldowns.ContainsKey(cooldownName))
        {
            activeCooldowns[cooldownName].Cancel();
            activeCooldowns[cooldownName].Dispose();
        }

        // Add cooldown to running cooldowns
        CancellationTokenSource cts = new CancellationTokenSource();
        activeCooldowns[cooldownName] = cts;

        // Invoke cooldown started event and start cooldown
        cooldownStartedEvents[cooldownName]?.Invoke();
        RunCooldown(cooldownName, duration, cts.Token);
    }

    /// <summary>
    /// Handles the cooldown duration asynchronously. If the cooldown is not canceled, it triggers the completion event when finished.
    /// </summary>
    /// <param name="cooldownName"> The name of the cooldown being tracked. </param>
    /// <param name="duration"> The duration of the cooldown in seconds. </param>
    /// <param name="token"> A cancellation token to allow the cooldown to be stopped early. </param>
    private async void RunCooldown(string cooldownName, float duration, CancellationToken token)
    {
        try
        {
            // Cooldown in seconds
            await Task.Delay((int)(duration * 1000), token);

            if (!token.IsCancellationRequested)
            {
                // Invoke cooldown completed event
                cooldownCompletedEvents[cooldownName]?.Invoke();
            }
        }
        catch (TaskCanceledException)
        {
            // Cooldown was canceled, do nothing
        }
    }
}
