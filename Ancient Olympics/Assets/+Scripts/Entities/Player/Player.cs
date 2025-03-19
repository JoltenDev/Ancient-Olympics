using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Player : NetworkEntity
{
    [Header("Player Fields")]
    [SerializeField] GameObject defaultPlayerHud;
    [SerializeField] float dodgeStrength = 5f;
    GameObject localHud = null;

    PlayerStates states;

    [Header("Base State")]
    PlayerBaseState currentState;
    public PlayerBaseState CurrentState { get { return currentState; } set { currentState = value; } }

    [Header("Input Handler")]
    PlayerInputHandler inputHandler = new PlayerInputHandler();
    public PlayerInputHandler InputHandler { get { return inputHandler; } }

    [Header("Cooldown Handler")]
    [SerializeField] PlayerCooldownHandler cooldownHandler;
    public PlayerCooldownHandler CooldownHandler { get { return cooldownHandler; } }

    Vector2 moveInput;
    public Vector2 MoveInput { get { return moveInput; } set { moveInput = value; } }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputHandler.Initialize();

        states = new PlayerStates(this);
        currentState = states.Idle();
        currentState.Enter();

        inputHandler.onMoveInput += SetMoveInput;
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        inputHandler.onMoveInput -= SetMoveInput;
        inputHandler.Dispose();
    }

    void Update()
    {
        if (!IsOwner) return;

        currentState.Update();

        Rotate();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        currentState.FixedUpdate();
    }

    void SetMoveInput(Vector2 input) => moveInput = input;

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

    [Rpc(SendTo.Server)]
    public void SendDodgeRpc(Vector3 direction)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dodgeDirection = (camRight * direction.x) + (camForward * direction.y);

        if (dodgeDirection.sqrMagnitude > 0.01f)
        {
            dodgeDirection.Normalize();
            rigidBody.AddForce(dodgeDirection * dodgeStrength, ForceMode.Impulse);
        }

        SyncPositionRpc(rigidBody.position);
    }

    void UpdateHud(float currentHealth)
    {
        if (!IsOwner) return;
        if (localHud == null) return;

        localHud.GetComponentInChildren<TMP_Text>().text = $"Health: {currentHealth}";
    }
}
