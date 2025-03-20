using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    public NetworkVariable<float> CurrentHealth { get { return currentHealth; } }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        ActionEvent.onDamage += Damage;
        ActionEvent.onHeal += Heal;

        currentHealth.OnValueChanged += (oldHealth, newHealth) =>
        {
            ActionEvent.onHealthChanged?.Invoke(newHealth);
        };
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        ActionEvent.onDamage -= Damage;
        ActionEvent.onHeal -= Heal;
    }

    public void Damage(float amount)
    {
        if (!IsOwner) return;

        SendDamageRpc(amount);
    }

    public void Heal(float amount)
    {
        if (!IsOwner) return;
        
        SendHealRpc(amount);
    }

    [Rpc(SendTo.Server)]
    public void SendDamageRpc(float amount)
    {
        if (!IsServer) return;

        currentHealth.Value = Mathf.Max(currentHealth.Value - amount, 0);
        CheckDeath();
    }

    [Rpc(SendTo.Server)]
    public void SendHealRpc(float amount)
    {
        if (!IsServer) return;

        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth);
    }

    void CheckDeath()
    {
        if (currentHealth.Value <= 0)
        {
            ActionEvent.onDeath?.Invoke();
        }
    }
}
