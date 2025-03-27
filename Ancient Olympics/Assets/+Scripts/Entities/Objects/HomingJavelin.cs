using Unity.Netcode;
using UnityEngine;

public class HomingJavelin : NetworkEntity
{
    public Transform target;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // Ensure only the owner moves it
    }

    public override void OnDestroy()
    {
        if (target != null)
        {
            if (target.GetComponentInChildren<NetworkHealth>().Immune)
            {
                target.GetComponentInChildren<WeaponHandler>().EquipWeaponClientRpc(1);
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, target.position);
        float stoppingDistance = 1.3f;

        if (distance > stoppingDistance)
        {
            // Move towards the target only if outside stopping range
            SendMove(direction, transform.position);
        }

        SendRotation(direction);
    }
}
