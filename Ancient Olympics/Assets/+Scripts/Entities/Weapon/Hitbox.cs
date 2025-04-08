using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Damage Inflicted")]
    [SerializeField] float damage;

    [Header("Destroy this object on hit?")]
    [SerializeField] bool destroyOnHit;

    [Header("Keep Hitbox Activated?")]
    public bool hitboxAlwaysActive;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (other.GetComponentInChildren<NetworkEntity>() == null) return; // If not entity, do nothing

            if (networkObject.OwnerClientId != GetComponentInParent<NetworkObject>().OwnerClientId)
            {
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
