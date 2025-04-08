using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Damage Fields")]
    [SerializeField] float damage;
    [SerializeField] float knockback;

    [Header("Destroy this object on hit?")]
    [SerializeField] bool destroyOnHit;

    [Header("Keep Hitbox Activated?")]
    public bool hitboxAlwaysActive;

    public void SetDamage(float damage, float knockback = 0)
    {
        this.damage = damage;
        this.knockback = knockback;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (other.GetComponentInChildren<NetworkEntity>() == null) return; // If not entity, do nothing

            var id = GetComponentInParent<NetworkObject>().OwnerClientId;
            if (networkObject.OwnerClientId != id)
            {
                other.GetComponentInParent<NetworkHealth>().SendDamageRpc(damage, id, networkObject.OwnerClientId, knockback);

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
