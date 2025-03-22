using UnityEngine;
using Unity.Netcode;

public class Player : NetworkEntity
{
    [SerializeField] string username;
    [SerializeField] GameObject weapon;

    [SerializeField] StandardData data;
    [SerializeField] NetworkHealth health;
    [SerializeField] PlayerCooldownHandler cooldownHandler;

    PlayerStates states;
    PlayerBaseState currentState;
    PlayerInputHandler inputHandler = new PlayerInputHandler();

    GameObject hitbox;
    Vector2 moveInput;
    float dodgeStrength = 10f;

    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public PlayerInputHandler InputHandler { get => inputHandler; }
    public PlayerCooldownHandler CooldownHandler { get => cooldownHandler; }
    public NetworkHealth NetworkHealth { get => health; }
    public Texture2D DefaultTexture => data.defaultTexture;
    public Texture2D DeathTexture => data.deathTexture;
    public Vector2 MoveInput { get => moveInput; set => moveInput = value; }

    void Start()
    {
        Cursor.SetCursor(DefaultTexture, Vector2.zero, CursorMode.Auto);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SendUsernameRpc(NetworkAccount.Username);

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
    }

    void Update()
    {
        if (!IsOwner) return;

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
        CooldownHandler.enabled = false;
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
}
