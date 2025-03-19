using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TMPro;

public class Player : NetworkEntity
{
    [Header("Player Fields")]
    [SerializeField] GameObject defaultPlayerHud;
    [SerializeField] PlayerCooldownHandler cooldownHandler;
    [SerializeField] float dodgeStrength = 5f;
    [SerializeField] float attackDuration = 0.5f;

    enum PlayerStates { Idle, Move, Dodge, Attack, Frozen }
    [SerializeField] PlayerStates state = PlayerStates.Idle;

    GameObject localHud = null;
    Controls controls;

    Vector2 moveInput;
    Vector3 moveDir;

    bool canDodge = true;

    System.Action dodgeStartedAction;
    System.Action dodgeCompletedAction;
    System.Action frozenStartedAction;
    System.Action frozenCompletedAction;
    System.Action<float> frozenStateChangeAction;
    System.Action<float> healthChangedAction;
    System.Action<InputAction.CallbackContext> leftClickAction;
    System.Action<float> startAttackCooldownAction;
    System.Action attackCooldownCompleted;

    bool canAttack = true;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        InitilizeControls();
        SubscribeActions();
        
        localHud = Instantiate(defaultPlayerHud, new Vector3(0, 2, 0), Quaternion.identity);
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        Debug.Log($"{gameObject.name} was destroyed!", this);

        controls.Player.OnSpace.performed -= Dodge;
        controls.Player.OnLeftClick.performed -= leftClickAction;
        UnsubscribeActions();
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

        leftClickAction = ctx => { if (canAttack) ChangeState(PlayerStates.Attack); };
        controls.Player.OnLeftClick.performed += leftClickAction;

        controls.Enable();
    }
    void SubscribeActions()
    {
        dodgeStartedAction = () => ChangeState(PlayerStates.Dodge);
        dodgeCompletedAction = () => canDodge = true;
        frozenStartedAction = () => ActionEvent.onFrozenStarted?.Invoke(.75f);
        frozenStateChangeAction = (float time) => ChangeState(PlayerStates.Frozen);
        frozenCompletedAction = () => ChangeState(PlayerStates.Idle);
        healthChangedAction = UpdateHud;

        // Dodge Actions
        ActionEvent.onDodgeStarted += dodgeStartedAction;
        ActionEvent.onDodgeCompleted += dodgeCompletedAction;
        ActionEvent.onDodgeStarted += frozenStartedAction;

        // Frozen Actions
        ActionEvent.onFrozenStarted += frozenStateChangeAction;
        ActionEvent.onFrozenCompleted += frozenCompletedAction;

        // Health Actions
        ActionEvent.onHealthChanged += healthChangedAction;

        // Attack Actions
        startAttackCooldownAction = (float amount) => { canAttack = false; };
        ActionEvent.onStartAttackCooldown += startAttackCooldownAction;

        attackCooldownCompleted = () => { canAttack = true; };
        ActionEvent.onAttackCooldownCompleted += attackCooldownCompleted;
    }
    void UnsubscribeActions()
    {
        ActionEvent.onDodgeStarted -= dodgeStartedAction;
        ActionEvent.onDodgeCompleted -= dodgeCompletedAction;
        ActionEvent.onDodgeStarted -= frozenStartedAction;

        ActionEvent.onFrozenStarted -= frozenStateChangeAction;
        ActionEvent.onFrozenCompleted -= frozenCompletedAction;

        ActionEvent.onHealthChanged -= healthChangedAction;
    }

    void Update()
    {
        if (!IsOwner) return;

        switch (state)
        {
            case PlayerStates.Frozen:
                moveDir = Vector3.zero;
                ActionEvent.onAnimatorMove?.Invoke("IsMoving", false); // Invokes movement animations
                return;
            
            case PlayerStates.Idle:
                if (moveInput != Vector2.zero) ChangeState(PlayerStates.Move);
                break;

            case PlayerStates.Move:
                moveDir = new Vector3(moveInput.x, 0, moveInput.y);
                ActionEvent.onAnimatorMove?.Invoke("IsMoving", moveInput != Vector2.zero); // Invokes movement animations

                if (moveInput == Vector2.zero) ChangeState(PlayerStates.Idle);
                break;

            case PlayerStates.Dodge:
                // Dodge logic
                break;

            case PlayerStates.Attack:
                // Attack logic
                ActionEvent.onFrozenStarted?.Invoke(attackDuration);
                break;
        }

        Rotate();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // (From local client [messenger]) Send movement direction to server
        SendMove(moveDir);
    }

    /// <summary>
    /// Changes the current state of the player
    /// </summary>
    /// <param name="newState"> The new state given to the player </param>
    void ChangeState(PlayerStates newState)
    {
        if (state == PlayerStates.Frozen && newState != PlayerStates.Idle)
            return;

        state = newState;
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

    void Dodge(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if (ctx.control.IsPressed() && moveInput != Vector2.zero && canDodge)
        {
            ActionEvent.onDodgeStarted?.Invoke(); // Start cooldown
            ActionEvent.onAnimatorDodge?.Invoke("dodge", .25f); // Invokes animation

            SendDodgeRpc(moveInput);
            canDodge = false;
        }
    }

    [Rpc(SendTo.Server)]
    void SendDodgeRpc(Vector3 direction)
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
