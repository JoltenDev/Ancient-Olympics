using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerCooldownHandler : NetworkBehaviour
{
    float dodgeCooldown = 1.5f;

    public override void OnNetworkSpawn()
    {
        ActionEvent.onDodgeStarted += StartDodgeCooldown;
    }

    public override void OnDestroy()
    {
        ActionEvent.onDodgeStarted -= StartDodgeCooldown;
    }

    /// <summary>
    /// Runs a cooldown timer asynchronously.
    /// </summary>
    async Task Cooldown(float cooldown)
    {
        await Task.Delay((int)(cooldown * 1000)); // Convert seconds to milliseconds
    }

    /// <summary>
    /// Starts the dodge cooldown and prevents reactivation until completed.
    /// </summary>
    async void StartDodgeCooldown()
    {
        await Cooldown(dodgeCooldown); // Wait until cooldown completes
        ActionEvent.onDodgeCompleted?.Invoke();
    }
}
