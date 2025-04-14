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
    public NetworkVariable<bool> Immune { get { return immune; } }
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
    public void SendDamageRpc(float amount, ulong attackerId, ulong victimId, float knockback = 0)
    {
        if (!IsServer) return;
        if (immune.Value) return;
        if (dead.Value) return;

        if (NetworkManager.Singleton.ConnectedClients[victimId].PlayerObject.TryGetComponent<Player>(out Player victim))
            victim.HitRpc(attackerId, knockback);

        currentHealth.Value = Mathf.Max(currentHealth.Value - amount, 0);
        CheckDeath(victimId);
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
        }
    }

    [Rpc(SendTo.Server)]
    public void SetImmunityRpc(ulong id, bool status)
    {
        if (!IsServer) return;

        NetworkManager.Singleton.ConnectedClients[id].PlayerObject
            .GetComponentInChildren<NetworkHealth>().Immune.Value = status;
    }

    [Rpc(SendTo.Server)]
    public void SetDeadRpc(ulong id, bool status)
    {
        if (!IsServer) return;

        NetworkManager.Singleton.ConnectedClients[id].PlayerObject
            .GetComponentInChildren<NetworkHealth>().Dead.Value = status;
    }
}
