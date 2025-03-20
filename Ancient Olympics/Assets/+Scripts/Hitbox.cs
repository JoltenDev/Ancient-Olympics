using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private ulong ownerClientId; // Store the owner ID (the attacker)

    public void SetOwner(ulong clientId)
    {
        ownerClientId = clientId; // Assign the attacker's ID when the hitbox is created
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject otherNetworkObject = other.GetComponent<NetworkObject>();

            if (otherNetworkObject != null && otherNetworkObject.OwnerClientId != ownerClientId)
            {
                other.GetComponentInParent<Player>().PlayerHitRpc();
                other.GetComponentInParent<NetworkHealth>()?.SendDamageRpc(5);
                gameObject.SetActive(false);
            }
        }
    }
}
