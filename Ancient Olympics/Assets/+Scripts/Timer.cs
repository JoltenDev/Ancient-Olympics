using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Timer
{
    public Dictionary<string, Action> timerStartedEvents = new Dictionary<string, Action>();
    public Dictionary<string, Action> timerCompletedEvents = new Dictionary<string, Action>();
    public Dictionary<string, CancellationTokenSource> activeTimers = new Dictionary<string, CancellationTokenSource>();
    public Dictionary<string, float> timerRemainingTimes = new Dictionary<string, float>();

    public void Dispose()
    {
        timerStartedEvents.Clear();
        timerCompletedEvents.Clear();
    }

    /// <summary>
    /// Registers a new timer type with an associated completion event.
    /// </summary>
    /// <param name="dict"> The dictionary storing timer events. </param>
    /// <param name="timerName"> The unique name of the timer to register. </param>
    /// <param name="onTimerComplete"> The action to invoke when the timer completes. </param>
    public void RegisterTimer(Dictionary<string, Action> dict, string timerName, Action onTimerComplete)
    {
        if (!dict.ContainsKey(timerName))
        {
            dict[timerName] = onTimerComplete;
        }
    }

    /// <summary>
    /// Starts a timer timer asynchronously. If a timer with the same name is already running, it cancels the existing one before starting a new timer.
    /// </summary>
    /// <param name="timerName"> The name of the timer to start. </param>
    /// <param name="duration"> The duration of the timer in seconds. </param>
    public void StartTimer(string timerName, float duration)
    {
        if (!timerCompletedEvents.ContainsKey(timerName)) return;

        // Cancel any existing timer with the same name
        if (activeTimers.ContainsKey(timerName))
        {
            activeTimers[timerName].Cancel();
            activeTimers[timerName].Dispose();
        }

        // Add timer to running timers
        CancellationTokenSource cts = new CancellationTokenSource();
        activeTimers[timerName] = cts;

        // Invoke timer started event and start timer
        if (timerStartedEvents.ContainsKey(timerName))
            timerStartedEvents[timerName]?.Invoke();
        RunTimer(timerName, duration, cts.Token);
    }

    /// <summary>
    /// Handles the timer duration asynchronously. If the timer is not canceled, it triggers the completion event when finished.
    /// </summary>
    /// <param name="timerName"> The name of the timer being tracked. </param>
    /// <param name="duration"> The duration of the timer in seconds. </param>
    /// <param name="token"> A cancellation token to allow the timer to be stopped early. </param>
    async void RunTimer(string timerName, float duration, CancellationToken token)
    {
        if (!timerRemainingTimes.ContainsKey(timerName))
            timerRemainingTimes[timerName] = duration;

        float startTime = Time.time;

        try
        {
            while (Time.time - startTime < duration)
            {
                if (token.IsCancellationRequested)
                    return;

                // Update the remaining time
                timerRemainingTimes[timerName] = duration - (Time.time - startTime);

                await Task.Delay(100, token); // Update every 100ms
            }

            if (!token.IsCancellationRequested)
            {
                timerRemainingTimes[timerName] = 0;

                if (timerCompletedEvents.ContainsKey(timerName))
                    timerCompletedEvents[timerName]?.Invoke();
            }
        }
        catch (TaskCanceledException)
        {
            // Timer was canceled
        }
    }

    /// <summary>
    /// Stops and cleans up a running timer by its name.
    /// </summary>
    /// <param name="timerName">The name of the timer to stop and clean up.</param>
    public void StopTimer(string timerName)
    {
        if (activeTimers.TryGetValue(timerName, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            activeTimers.Remove(timerName);
        }

        if (timerRemainingTimes.ContainsKey(timerName))
        {
            timerRemainingTimes.Remove(timerName);
        }
    }

    /// <summary>
    /// Stops and cleans up all active timers.
    /// </summary>
    public void StopAllTimers()
    {
        foreach (var cts in activeTimers.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }

        activeTimers.Clear();
        timerRemainingTimes.Clear();
    }
}
