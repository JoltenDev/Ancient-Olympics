using UnityEngine;
using Unity.Netcode;
using System.Collections;

public abstract class NetworkEntity : NetworkBehaviour
{
    [Header("Entity Fields")]
    [SerializeField] protected Rigidbody rigidBody;
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float rotationSpeed = 5f;

    public Rigidbody Rigidbody { get { return rigidBody; } }
    public float Speed { get { return speed; } }

    Vector3 networkPos;
    Vector3 networkLinearVelocity;
    Vector3 estimatedPos;

    Quaternion networkRot;
    Vector3 networkAngularVelocity;
    Quaternion estimatedRot;

    float lastUpdatedTime;
    float posErrorThreshold = 0.1f;
    float rotErrorThreshold = 35f;

    protected virtual void FixedUpdate()
    {
        if (!IsOwner)
        {
            ExtrapolatePosition();
            ExtrapolateRotation();
        }
    }

    [Rpc(SendTo.Server)]
    public void SendPositionRpc(Vector3 position, Vector3 velocity)
    {
        if (!IsServer) return;

        networkPos = position;
        networkLinearVelocity = velocity;

        SyncPositionRpc(position, velocity, NetworkManager.Singleton.ServerTime.TimeAsFloat);
    }

    [Rpc(SendTo.Server)]
    public void SendRotationRpc(Quaternion rotation, Vector3 velocity)
    {
        if (!IsServer) return;

        networkRot = rotation;
        networkAngularVelocity = velocity;

        SyncRotationRpc(rotation, velocity, NetworkManager.Singleton.ServerTime.TimeAsFloat);
    }

    [Rpc(SendTo.Everyone)]
    public void SyncPositionRpc(Vector3 position, Vector3 velocity, float serverTime)
    {
        if (IsOwner) return;

        networkPos = position;
        networkLinearVelocity = velocity;

        lastUpdatedTime = serverTime;
    }

    [Rpc(SendTo.Everyone)]
    public void SyncRotationRpc(Quaternion rotation, Vector3 velocity, float serverTime)
    {
        if (IsOwner) return;

        networkRot = rotation;
        networkAngularVelocity = velocity;

        lastUpdatedTime = serverTime;
    }

    public void ExtrapolatePosition()
    {
        estimatedPos = networkPos + networkLinearVelocity * (NetworkManager.Singleton.ServerTime.TimeAsFloat - lastUpdatedTime);

        Vector3 posError = estimatedPos - rigidBody.position;
        if (posError.magnitude > posErrorThreshold)
        {
            rigidBody.position = Vector3.Lerp(rigidBody.position, estimatedPos, Time.fixedDeltaTime * 10);
        }

        rigidBody.linearVelocity = networkLinearVelocity;
    }

    public void ExtrapolateRotation()
    {
        float timeSinceUpdate = NetworkManager.Singleton.ServerTime.TimeAsFloat - lastUpdatedTime;

        Vector3 angularVelocityInDegrees = networkAngularVelocity * Mathf.Rad2Deg;

        estimatedRot = networkRot * Quaternion.Euler(angularVelocityInDegrees * timeSinceUpdate);

        float angleError = Quaternion.Angle(rigidBody.rotation, estimatedRot);
        if (angleError > rotErrorThreshold)
        {
            rigidBody.rotation = Quaternion.Slerp(rigidBody.rotation, estimatedRot.normalized, timeSinceUpdate * 10f);
        }

        rigidBody.rotation = networkRot.normalized;
    }

    public void ApplyRotate(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        // Flatten to XZ
        direction.y = 0;
        direction.Normalize();

        // Get current facing direction
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        // Calculate signed angle between current forward and desired direction
        float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

        // Convert to angular velocity (radians per second)
        float angleInRadians = angle * Mathf.Deg2Rad;
        float desiredAngularVelocityY = angleInRadians / Time.fixedDeltaTime;

        // Optionally clamp rotation speed
        float maxAngularSpeed = 10f; // in radians/sec
        desiredAngularVelocityY = Mathf.Clamp(desiredAngularVelocityY, -maxAngularSpeed, maxAngularSpeed);

        // Apply angular velocity only around Y-axis
        rigidBody.angularVelocity = new Vector3(0f, desiredAngularVelocityY, 0f);
    }

    [Rpc(SendTo.Owner)]
    public void TeleportRpc(Vector3 pos)
    {
        StartCoroutine(Teleport(pos));
    }

    [Rpc(SendTo.Server)]
    public void DestroyProjectileRpc()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw client's view of the object
        Gizmos.color = IsOwner ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Optionally, draw networked position (server's or synced version)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(networkPos, 0.2f);
    }

    IEnumerator Teleport(Vector3 pos)
    {
        posErrorThreshold = 0f;

        rigidBody.position = pos;
        estimatedPos = pos;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.05f);

        posErrorThreshold = 0.1f;
    }
}
