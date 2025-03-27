using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    
    bool immune;
    bool dead;

    public bool Immune { get => immune; set => immune = value; }
    public bool Dead { get => dead; set => dead = value; }
    public float MaxHealth { get => maxHealth; }
    public NetworkVariable<float> CurrentHealth { get { return currentHealth; } }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SendHealRpc(maxHealth);

        currentHealth.OnValueChanged += (oldHealth, newHealth) =>
        {
            ActionEvent.onHealthChanged?.Invoke(newHealth);
        };

        dead = false;
    }

    [Rpc(SendTo.Server)]
    public void SendDamageRpc(float amount, ulong id)
    {
        if (!IsServer) return;
        if (immune) return;

        if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject.TryGetComponent<Player>(out Player player)) player.SendHitRpc();

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
            if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject.TryGetComponent<Player>(out Player player)) player.SendDeathRpc();
            dead = true;
        }
    }
}
