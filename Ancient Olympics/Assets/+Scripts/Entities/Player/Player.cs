using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class Player : NetworkEntity
{
    [SerializeField] PlayerCooldownHandler cooldownHandler;
    Controls controls;

    Vector2 moveInput;
    Vector3 moveDir;

    bool canDodge = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        InitilizeControls();

        ActionEvent.onDodgeCompleted += delegate { canDodge = true; };
    }

    public override void OnDestroy()
    {
        Debug.Log($"{gameObject.name} was destroyed!", this);

        controls.Player.OnSpace.performed -= Dodge;
        ActionEvent.onDodgeCompleted -= delegate { canDodge = true; };
    }

    void Update()
    {
        if (!IsOwner) return;

        Rotate();
        networkAnimatorSync.SyncAnimation(moveInput);

        // Apply move input to world space
        moveDir = new Vector3(moveInput.x, 0, moveInput.y);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // (From local client [messenger]) Send movement direction to server
        SendMove(moveDir);
    }

    /// <summary>
    /// Rotates player towards the mouse position in world space
    /// </summary>
    void Rotate()
    {
        // Capture mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // Get the direction of the mouse position
            Vector3 direction = (hit.point - transform.position).normalized;
            direction.y = 0;

            // (From local client [messenger]) Send rotation direction to server
            SendRotation(direction);
        }
    }

    void InitilizeControls()
    {
        controls = new Controls();

        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        controls.Player.OnSpace.performed += Dodge;

        controls.Enable();
    }

    void Dodge(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if (ctx.control.IsPressed())
            SendDodgeRpc(moveInput);
    }

    [Rpc(SendTo.Server)]
    void SendDodgeRpc(Vector3 direction)
    {
        if (!canDodge) return;

        // Get the camera's forward and right vectors (ignoring vertical rotation)
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Determine dodge direction based on movement input priority
        Vector3 dodgeDirection = Vector3.zero;
        if (direction.x > 0)
            dodgeDirection = camRight;    // Dodge Right
        else if (direction.x < 0)
            dodgeDirection = -camRight;   // Dodge Left
        else if (direction.y > 0)
            dodgeDirection = camForward;  // Dodge Forward
        else if (direction.y < 0)
            dodgeDirection = -camForward; // Dodge Backward

        // Apply the dodge force
        if (dodgeDirection != Vector3.zero)
        {
            rigidBody.AddForce(dodgeDirection * 5, ForceMode.Impulse);
        }

        // Start cooldown
        ActionEvent.onDodgeStarted?.Invoke();
        canDodge = false;

        SyncPositionRpc(rigidBody.position);
    }
}
