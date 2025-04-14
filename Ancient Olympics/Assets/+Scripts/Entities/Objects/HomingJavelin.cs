using Unity.Netcode;
using UnityEngine;

public class HomingJavelin : NetworkEntity
{
    Transform target;
    ulong targetId = 100;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
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

    protected override void FixedUpdate()
    {
        if (target == null) return;

        base.FixedUpdate();

        if (IsOwner)
        {
            // Position
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f;

            float distance = Vector3.Distance(transform.position, target.position);
            float stoppingDistance = 1.1f;

            if (distance > stoppingDistance)
            {
                // Move towards the target only if outside stopping range
                Vector3 targetVelocity = direction * speed;
                rigidBody.linearVelocity = Vector3.Lerp(rigidBody.linearVelocity, targetVelocity, 5 * Time.fixedDeltaTime);
            }
            else
            {
                Vector3 targetVelocity = -direction * speed;
                rigidBody.linearVelocity = Vector3.Lerp(rigidBody.linearVelocity, targetVelocity, 5 * Time.fixedDeltaTime);
            }
            SendPositionRpc(rigidBody.position, rigidBody.linearVelocity);


            // Rotation
            if (direction != Vector3.zero)
            {
                ApplyRotate(direction);
                SendRotationRpc(rigidBody.rotation, rigidBody.angularVelocity);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void AddSpeedRpc(float amount)
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
