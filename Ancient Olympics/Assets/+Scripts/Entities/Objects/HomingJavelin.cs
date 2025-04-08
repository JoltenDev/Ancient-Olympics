using Unity.Netcode;
using UnityEngine;

public class HomingJavelin : NetworkEntity
{
    Transform target;
    ulong targetId = 100;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // Ensure only the owner moves it
    }

    public override void OnDestroy()
    {
        if (target != null && targetId != 100)
        {
            if (target.GetComponentInChildren<NetworkHealth>().Immune.Value)
            {
                target.GetComponentInChildren<WeaponHandler>().EquipWeaponRpc(targetId, 2);
                GameManager.Instance.SetJavelinWielderRpc(targetId);
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        float distance = Vector3.Distance(transform.position, target.position);
        float stoppingDistance = 1.1f;

        if (distance > stoppingDistance)
        {
            // Move towards the target only if outside stopping range
            SendMove(direction, transform.position);
        } 
        else {
            SendMove(-direction, transform.position);
        }

        SendRotation(direction);
    }

    public void AddSpeed(float amount)
    {
        speed += amount;
    }

    [Rpc(SendTo.Everyone)]
    public void SetTargetRpc(ulong targetId)
    {
        this.targetId = targetId;
        target = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject.transform;
    }
}
