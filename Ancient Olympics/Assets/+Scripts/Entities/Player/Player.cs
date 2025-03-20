using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.Rendering;

public class Player : NetworkEntity
{
    [Header("Username")]
    [SerializeField] string username;
    public string Username { get { return username; } set { username = value; } }

    [Header("Player Fields")]
    [SerializeField] GameObject defaultPlayerHud;
    [SerializeField] float dodgeStrength = 5f;
    GameObject localHud = null;
    [SerializeField] GameObject hitbox;

    [Header("Cursor Textures")]
    [SerializeField] Texture2D defaultTexture;
    public Texture2D DefaultTexture { get { return defaultTexture; } }
    [SerializeField] Texture2D deathTexture;
    public Texture2D DeathTexture { get { return deathTexture; } }

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

    [Header("Network Health")]
    [SerializeField] NetworkHealth networkHealth;
    public NetworkHealth NetworkHealth { get { return networkHealth; } }

    Vector2 moveInput;
    public Vector2 MoveInput { get { return moveInput; } set { moveInput = value; } }

    void Start()
    {
        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SetNameRpc(PlayerManager.username);

        states = new PlayerStates(this);
        currentState = states.Idle();
        currentState.Enter();

        //localHud = Instantiate(defaultPlayerHud);

        ActionEvent.onHealthChanged += UpdateHud;
        ActionEvent.onSwingStarted += ActivateHitbox;
        ActionEvent.onSwingCompleted += DeactivateHitbox;

        inputHandler.Initialize();
        inputHandler.onMoveInput += SetMoveInput;

        hitbox.GetComponent<Hitbox>().SetOwner(OwnerClientId); // Set the attacker’s client ID
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        Destroy(localHud);
        ActionEvent.onHealthChanged -= UpdateHud;
        ActionEvent.onSwingStarted -= ActivateHitbox;
        ActionEvent.onSwingCompleted -= DeactivateHitbox;

        inputHandler.onMoveInput -= SetMoveInput;
        inputHandler.Dispose();
    }

    void Update()
    {
        if (!IsOwner) return;

        currentState.Update();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        currentState.FixedUpdate();

        Rotate();
    }

    void SetMoveInput(Vector2 input) => moveInput = input;

    [Rpc(SendTo.Everyone)]
    public void PlayerHitRpc() => currentState?.SwitchState(states?.Hit());

    void ActivateHitbox() => hitbox.SetActive(true);
    void DeactivateHitbox() => hitbox.SetActive(false);

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

    [Rpc(SendTo.Everyone)]
    void SetNameRpc(string name)
    {
        username = name;
        this.name = $"Player ({name})";
    }

    [Rpc(SendTo.Everyone)]
    public void DisablePlayerRpc()
    {
        CooldownHandler.enabled = false;
        enabled = false;
    }
}
