using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Damage Inflicted")]
    [SerializeField] float damage;

    [Header("Attacker's ID (Set when activated)")]
    [SerializeField] ulong ownerClientId; // Store the owner ID (the attacker)

    [Header("Destroy this object on hit?")]
    [SerializeField] bool destroyOnHit;
    
    public void SetOwner(ulong clientId)
    {
        ownerClientId = clientId; // Assign the attacker's ID when the hitbox is created
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (other.GetComponentInChildren<NetworkEntity>() == null) return; // If not entity, do nothing

            if (networkObject.OwnerClientId != ownerClientId)
            {
                if (other.GetComponent<NetworkHealth>().Dead) return; // If entity is dead, do nothing

                other.GetComponentInParent<NetworkHealth>()?.SendDamageRpc(damage, networkObject.OwnerClientId);

                gameObject.SetActive(false);
                if (destroyOnHit)
                    DestroyProjectile();
            }
        }
    }

    [Rpc(SendTo.Server)]
    void DestroyProjectile()
    {
        NetworkObject.Destroy(transform.parent.parent.gameObject);
    }
}
