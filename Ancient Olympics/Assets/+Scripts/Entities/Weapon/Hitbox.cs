using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Damage Inflicted")]
    [SerializeField] float damage;

    [Header("Destroy this object on hit?")]
    [SerializeField] bool destroyOnHit;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (other.GetComponentInChildren<NetworkEntity>() == null) return; // If not entity, do nothing

            if (networkObject.OwnerClientId != GetComponentInParent<NetworkObject>().OwnerClientId)
            {
                if (other.GetComponentInParent<NetworkHealth>().Dead.Value) return; // If entity is dead, do nothing

                other.GetComponentInParent<NetworkHealth>().SendDamageRpc(damage, networkObject.OwnerClientId);

                gameObject.SetActive(false);
                if (destroyOnHit)
                    DestroyProjectile();
            }
        }
    }

    void DestroyProjectile()
    {
        transform.GetComponentInParent<NetworkEntity>().DestroyProjectileRpc();
    }
}
