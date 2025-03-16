using UnityEngine;
using Unity.Netcode;

public class Entity : NetworkBehaviour
{
    [SerializeField] float speed = 15f;
    Rigidbody rigidBody;
    Animator animator;
    Controls controls;

    Vector2 moveInput;
    Vector3 moveDir;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        controls = new Controls();
        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        controls.Enable();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Capture mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 target = hit.point;

            // Get the direction of the mouse position
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                // (From local client [messenger]) Send rotation direction to server
                SendRotateRpc(direction);
            }
        }

        // Apply move input to world space
        moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        AnimateRpc(moveInput);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // (From local client [messenger]) Send movement direction to server
        SendMoveRpc(moveDir);
    }

    [Rpc(SendTo.Server)]
    void SendMoveRpc(Vector3 moveDir)
    {
        if (IsServer)
        {
            // Apply new position on server
            rigidBody.AddForce(moveDir * speed, ForceMode.Force);

            // (From server) Sync movement with all clients
            ReceiveMoveRpc(rigidBody.position);
        }
    }

    [Rpc(SendTo.NotServer)]
    void ReceiveMoveRpc(Vector3 syncedPos)
    {
        if (!IsOwner)
        {
            // (From client excluding messenger) Sync transform of messenger client sent by server
            rigidBody.position = Vector3.Lerp(rigidBody.position, syncedPos, 0.1f);
        }
    }

    [Rpc(SendTo.Server)]
    void SendRotateRpc(Vector3 direction)
    {
        // Rotate player towards direction of mouse
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, 0.1f));

        // (From server) Sync rotation with all clients
        SyncRotateRpc(targetRotation);
    }

    [Rpc(SendTo.NotServer)]
    void SyncRotateRpc(Quaternion syncedPos)
    {
        if (!IsOwner)
        {
            // (From client excluding messenger) Sync rotation of messenger client sent by server
            rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, syncedPos, 0.1f);
        }
    }

    [Rpc(SendTo.Everyone)]
    void AnimateRpc(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
            animator.SetBool("IsMoving", true);
        else
            animator.SetBool("IsMoving", false);
    }
}
