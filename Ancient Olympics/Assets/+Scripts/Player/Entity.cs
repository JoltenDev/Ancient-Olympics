using UnityEngine;
using Unity.Netcode;

public class Entity : NetworkBehaviour
{
    [SerializeField] float speed;

    Rigidbody rigidBody;
    Controls controls;

    Vector2 moveInput;
    Vector3 direction;

    void Awake()
    {
        if (!IsOwner) return;

        rigidBody = GetComponent<Rigidbody>();
        controls = new Controls();

        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        controls.Enable();
    }
    public override void OnNetworkDespawn() 
    {
        if (!IsOwner) return;

        controls.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        direction = new Vector3(moveInput.x, rigidBody.linearVelocity.y, moveInput.y);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rigidBody.linearVelocity = direction * speed;
        MoveServerRpc(transform.position);
    }

    [ServerRpc]
    void MoveServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
