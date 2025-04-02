using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public abstract class NetworkEntity : NetworkBehaviour
{
    [Header("Entity Fields")]
    [SerializeField] protected Rigidbody rigidBody;
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float rotationSpeed = 5f;

    /// <summary>
    /// Sends movement input from the client to the server for synchronization.
    /// Only the owning client can call this.
    /// </summary>
    /// <param name="destination">The target movement position.</param>
    public void SendMove(Vector3 destination, Vector3 currentPosition)
    {
        if (IsOwner)
            SendRpc(nameof(SyncPositionRpc), destination, currentPosition);
    }

    /// <summary>
    /// Sends rotation input from the client to the server for synchronization.
    /// Only the owning client can call this.
    /// </summary>
    /// <param name="direction">The target look direction.</param>
    public void SendRotation(Vector3 direction)
    {
        if (IsOwner)
            SendRpc(nameof(SyncRotationRpc), direction, Vector3.zero);
    }

    /// <summary>
    /// Sends a RPC from the client to the server.
    /// This function determines whether to sync movement or rotation based on the function name.
    /// </summary>
    /// <param name="functionName">The name of the RPC function to invoke.</param>
    /// <param name="data">The vector data for the function (movement direction or rotation target).</param>
    [Rpc(SendTo.Server)]
    void SendRpc(string functionName, Vector3 data, Vector3 currentPosition)
    {
        if (IsServer)
        {
            if (functionName == nameof(SyncPositionRpc))
            {
                rigidBody.MovePosition(Vector3.Lerp(rigidBody.position, currentPosition + data * speed * Time.fixedDeltaTime, 5 * Time.fixedDeltaTime));
                SyncPositionRpc(rigidBody.position);
            }
            else if (functionName == nameof(SyncRotationRpc))
            {
                Quaternion targetRotation = Quaternion.LookRotation(data);
                rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, 0.1f));
                SyncRotationRpc(targetRotation);
            }
        }
    }

    /// <summary>
    /// Synchronizes movement from the server to all clients.
    /// </summary>
    /// <param name="syncedPos">The updated position received from the server.</param>
    [Rpc(SendTo.NotServer)]
    public void SyncPositionRpc(Vector3 syncedPos)
    {
        if (!IsOwner)
            rigidBody.position = Vector3.Lerp(rigidBody.position, syncedPos, 0.1f);
    }

    /// <summary>
    /// Synchronizes rotation from the server to all clients.
    /// </summary>
    /// <param name="syncedRotation">The updated rotation received from the server.</param>
    [Rpc(SendTo.NotServer)]
    public void SyncRotationRpc(Quaternion syncedRotation)
    {
        if (!IsOwner)
            rigidBody.rotation = Quaternion.Slerp(rigidBody.rotation, syncedRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}
