using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

public class Player : NetworkEntity
{
    [SerializeField] string username;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject nameUI;
    [SerializeField] GameObject hud;

    [SerializeField] StandardData data;
    [SerializeField] NetworkHealth health;

    PlayerStates states;
    PlayerBaseState currentState;
    PlayerInputHandler inputHandler = new PlayerInputHandler();
    Timer cooldownHandler = new Timer();

    GameObject hitbox;
    Vector2 moveInput;
    float dodgeStrength = 10f;

    public string Username { get => username; }
    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public PlayerInputHandler InputHandler { get => inputHandler; }
    public Timer CooldownHandler { get => cooldownHandler; }
    public NetworkHealth NetworkHealth { get => health; }
    public Texture2D DefaultTexture => data.defaultTexture;
    public Texture2D DeathTexture => data.deathTexture;
    public Vector2 MoveInput { get => moveInput; set => moveInput = value; }

    public Action onDodgeCooldownStarted;
    public Action onFrozenCooldownStarted;
    public Action onAttackDurationStarted;
    public Action onComboWindowStarted;
    public Action onHitStarted;

    public Action onDodgeCooldownCompleted;
    public Action onFrozenCooldownCompleted;
    public Action onAttackDurationCompleted;
    public Action onComboWindowCompleted;
    public Action onHitCompleted;

    void Start()
    {
        Cursor.SetCursor(DefaultTexture, Vector2.zero, CursorMode.Auto);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SendUsernameRpc(NetworkAccount.Username);
        UIManager.Instance.Hud = hud;

        states = new PlayerStates(this);
        CurrentState = states.Idle();
        CurrentState.Enter();

        //localHud = Instantiate(defaultPlayerHud);

        ActionEvent.onHealthChanged += UpdateHud;
        //ActionEvent.onSwingStarted += ActivateHitbox;
        //ActionEvent.onSwingCompleted += DeactivateHitbox;

        inputHandler.Initialize();
        inputHandler.onMoveInput += SetMoveInput;

        //Weapon.GetComponentInChildren<Hitbox>().SetOwner(OwnerClientId); // Set the attacker’s client ID
        //hitbox = Weapon.GetComponentInChildren<Hitbox>().gameObject;

        // Register cooldowns
        CooldownHandler.RegisterTimer(CooldownHandler.timerStartedEvents, "Dodge", () => onDodgeCooldownStarted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerStartedEvents, "Frozen", () => onFrozenCooldownStarted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerStartedEvents, "Attack Duration", () => onAttackDurationStarted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerStartedEvents, "Combo Window", () => onComboWindowStarted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerStartedEvents, "Hit", () => onHitStarted?.Invoke());

        // Register cooldowns
        CooldownHandler.RegisterTimer(CooldownHandler.timerCompletedEvents, "Dodge", () => onDodgeCooldownCompleted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerCompletedEvents, "Frozen", () => onFrozenCooldownCompleted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerCompletedEvents, "Attack Duration", () => onAttackDurationCompleted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerCompletedEvents, "Combo Window", () => onComboWindowCompleted?.Invoke());
        CooldownHandler.RegisterTimer(CooldownHandler.timerCompletedEvents, "Hit", () => onHitCompleted?.Invoke());
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        //Destroy(localHud);
        ActionEvent.onHealthChanged -= UpdateHud;
        //ActionEvent.onSwingStarted -= ActivateHitbox;
        //ActionEvent.onSwingCompleted -= DeactivateHitbox;

        inputHandler.onMoveInput -= SetMoveInput;
        inputHandler.Dispose();

        CooldownHandler.Dispose();
    }

    void Update()
    {
        if (!IsOwner) return;

        SetPlayerUIRpc(username);
        if (nameUI != null && Camera.main != null)
        {
            // Get direction to camera
            Vector3 direction = nameUI.transform.position - Camera.main.transform.position;

            // Keep the UI upright by zeroing out the Y component (so it doesn't rotate weirdly)
            direction.y = 0;

            // Apply rotation while maintaining a slight tilt toward the camera
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(15f, 0f, 0f); // Adjust tilt angle as needed
            MoveUIRpc(targetRotation);
        }

        CurrentState.Update();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        CurrentState.FixedUpdate();

        Rotate();
    }

    void SetMoveInput(Vector2 input) => moveInput = input;

    [Rpc(SendTo.Everyone)]
    public void PlayerHitRpc() => CurrentState?.SwitchState(states?.Hit());

    //void ActivateHitbox() => hitbox.SetActive(true);
    //void DeactivateHitbox() => hitbox.SetActive(false);

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
            SendRotation(direction * Time.fixedDeltaTime);
        }
    }

    [Rpc(SendTo.Server)]
    public void SendDodgeRpc(Vector3 direction, Vector3 forward, Vector3 right)
    {
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 dodgeDirection = (right * direction.x) + (forward * direction.y);

        rigidBody.AddForce(dodgeDirection * dodgeStrength, ForceMode.Impulse);

        SyncPositionRpc(rigidBody.position);
    }

    void UpdateHud(float currentHealth)
    {
        /*
        if (!IsOwner) return;
        if (localHud == null) return;

        localHud.GetComponentInChildren<TMP_Text>().text = $"Health: {currentHealth}";
        */
    }

    [Rpc(SendTo.Everyone)]
    public void DisablePlayerRpc()
    {
        enabled = false;
    }

    [Rpc(SendTo.Everyone)]
    void SendUsernameRpc(string username, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        this.username = username;
        gameObject.name = $"Player ({username})";

        Debug.Log($"Client {clientId} set their username to {username}");
        // Store the username for this client in a dictionary (optional)
    }

    [Rpc(SendTo.Everyone)]
    public void SetPlayerUIRpc(string username)
    {
        nameUI.GetComponentInChildren<TMP_Text>().text = username;
    }

    [Rpc(SendTo.Everyone)]
    public void MoveUIRpc(Quaternion targetRotation)
    {
        if (nameUI.transform == null) return;

        nameUI.transform.rotation = targetRotation;
    }
}
