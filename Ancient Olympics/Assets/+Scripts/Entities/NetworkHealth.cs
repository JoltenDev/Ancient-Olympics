using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    [Header("Current Status")]
    [SerializeField] NetworkVariable<bool> immune = new NetworkVariable<bool>();
    [SerializeField] NetworkVariable<bool> dead = new NetworkVariable<bool>();

    public float MaxHealth { get => maxHealth; }
    public NetworkVariable<float> CurrentHealth { get { return currentHealth; } }
    public NetworkVariable<bool> Immune { get { return dead; } }
    public NetworkVariable<bool> Dead { get { return dead; } }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SendHealRpc(maxHealth);

        currentHealth.OnValueChanged += (oldHealth, newHealth) =>
        {
            ActionEvent.onHealthChanged?.Invoke(newHealth);
        };
    }

    [Rpc(SendTo.Server)]
    public void SendDamageRpc(float amount, ulong id)
    {
        if (!IsServer) return;
        if (immune.Value) return;

        if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject.TryGetComponent<Player>(out Player player)) 
            player.HitRpc();

        currentHealth.Value = Mathf.Max(currentHealth.Value - amount, 0);
        CheckDeath(id);
    }

    [Rpc(SendTo.Server)]
    public void SendHealRpc(float amount)
    {
        if (!IsServer) return;

        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth);
    }

    void CheckDeath(ulong id)
    {
        if (currentHealth.Value <= 0)
        {
            if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject.TryGetComponent<Player>(out Player player)) 
                player.DeathRpc();
            dead.Value = true;
        }
    }
}
