using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

public class Player : NetworkEntity
{
    [Header("Player Information")]
    [SerializeField] string username;
    [SerializeField] WeaponHandler weaponHandler;

    [Header("UI Elements")]
    [SerializeField] GameObject nameUI;
    [SerializeField] GameObject hud;

    [Header("Player Data")]
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
    public WeaponHandler WeaponHandler { get => weaponHandler; }
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

        if (UIManager.Instance != null)
            UIManager.Instance.Hud = hud;

        states = new PlayerStates(this);
        CurrentState = states.Idle();
        CurrentState.Enter();

        inputHandler.Initialize();
        inputHandler.onMoveInput += SetMoveInput;

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

        inputHandler.onMoveInput -= SetMoveInput;
        inputHandler.Dispose();

        CooldownHandler.Dispose();
    }
    
    void Update()
    {
        if (!IsOwner) return;

        SetPlayerUIRpc(username);
        RotateUI();

        CurrentState.Update();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        CurrentState.FixedUpdate();

        Rotate();
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
            SendRotation(direction * Time.fixedDeltaTime);
        }
    }

    void RotateUI()
    {
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

    [Rpc(SendTo.Server)]
    public void SpawnJavelinProjectileRpc(ulong clientID, Vector3 position, Vector3 direction)
    {
        GameObject projectilePrefab = WeaponHandler.projectiles[0];

        Vector3 spawnPos = new Vector3(position.x, 0.4f, position.z);
        Quaternion spawnRot = Quaternion.LookRotation(direction, Vector3.up);

        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, spawnRot); // Instantiate Projectile
        projectileInstance.GetComponentInChildren<Hitbox>().SetOwner(OwnerClientId); // Set attacker to this client

        NetworkObject networkObj = projectileInstance.GetComponent<NetworkObject>();
        networkObj.Spawn(); // Spawn Projectile

        projectileInstance.GetComponent<HomingJavelin>().target = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.transform; // Target other client
    }

    [Rpc(SendTo.Everyone)]
    public void SendIdleRpc() => CurrentState?.SwitchState(states?.Idle());

    [Rpc(SendTo.Everyone)]
    public void SendHitRpc() => CurrentState?.SwitchState(states?.Hit());

    // On Death
    [Rpc(SendTo.Everyone)]
    public void SendDeathRpc() => CurrentState?.SwitchState(states?.Death());

    [Rpc(SendTo.Everyone)]
    public void DisablePlayerRpc()
    {
        enabled = false;
    }

    // UI Elements
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
