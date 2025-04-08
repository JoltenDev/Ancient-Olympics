using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections;

public class Player : NetworkEntity
{
    [Header("Player Fields")]
    [SerializeField] string username;
    [SerializeField] WeaponHandler weaponHandler;
    [SerializeField] NetworkAnimatorSync networkAnimatorSync;

    [Header("UI Elements")]
    [SerializeField] GameObject nameUI;
    [SerializeField] GameObject hud;

    [Header("Player Data")]
    [SerializeField] StandardData data;
    [SerializeField] NetworkHealth health;

    [Header("Extra")]
    [SerializeField] GameObject horse;

    PlayerStates states;
    PlayerBaseState currentState;
    PlayerInputHandler inputHandler = new PlayerInputHandler();
    Timer cooldownHandler = new Timer();

    GameObject hitbox;
    Vector2 moveInput;
    float dodgeStrength = 10f;

    #region Getters/Setters
    public string Username { get => username; }
    public WeaponHandler WeaponHandler { get => weaponHandler; }
    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public PlayerStates States { get => states; }
    public PlayerInputHandler InputHandler { get => inputHandler; }
    public GameObject Hud { get => hud; }
    public GameObject Horse { get => horse; }
    public Timer CooldownHandler { get => cooldownHandler; }
    public NetworkHealth NetworkHealth { get => health; }
    public Texture2D DefaultTexture => data.defaultTexture;
    public Texture2D DeathTexture => data.deathTexture;
    public Vector2 MoveInput { get => moveInput; set => moveInput = value; }
    #endregion

    #region Actions
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

    public Action<string, bool> onAnimatorSetBool;
    public Action<string, float> onAnimatorCrossFade;
    #endregion

    void Start()
    {
        Cursor.SetCursor(DefaultTexture, Vector2.zero, CursorMode.Auto);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SendUsernameRpc(NetworkAccount.Username);
        SetPlayerUIRpc(username);

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

        // Animator
        onAnimatorSetBool += networkAnimatorSync.AnimateSetBoolRpc;
        onAnimatorCrossFade += networkAnimatorSync.AnimateCrossFadeRpc;

        // Health UI
        NetworkHealth.CurrentHealth.OnValueChanged += HealthUI;
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        inputHandler.onMoveInput -= SetMoveInput;
        inputHandler.Dispose();

        CooldownHandler.Dispose();

        onAnimatorSetBool -= networkAnimatorSync.AnimateSetBoolRpc;
        onAnimatorCrossFade -= networkAnimatorSync.AnimateCrossFadeRpc;

        NetworkHealth.CurrentHealth.OnValueChanged -= HealthUI;
    }
    
    void Update()
    {
        if (!IsOwner) return;

        RotateUI();

        CurrentState.Update();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        CurrentState.FixedUpdate();

        if (!health.Dead.Value)
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

    void HealthUI(float prevValue, float newValue)
    {
        StartCoroutine(SmoothHealthLerp(prevValue, newValue));
    }

    IEnumerator SmoothHealthLerp(float from, float to)
    {
        Slider slider = hud.GetComponent<HudItems>().health.GetComponentInChildren<Slider>();
        float elapsed = 0f;
        float duration = 0.5f; // Adjust for speed

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        slider.value = to; // snap to final value just in case
    }

    #region RPCs
    #region Specifics
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
    public void SpawnJavelinProjectileRpc(ulong ownerId, ulong targetId, Vector3 position, Vector3 direction)
    {
        GameObject projectilePrefab = WeaponHandler.projectiles[0];

        Vector3 spawnPos = new Vector3(position.x, 0.2f, position.z);
        Quaternion spawnRot = Quaternion.LookRotation(direction, Vector3.up);

        NetworkObject networkObj = projectilePrefab.GetComponent<NetworkObject>();
        var clone = networkObj.InstantiateAndSpawn(NetworkManager.Singleton, ownerId, false, false, false, spawnPos, spawnRot);

        clone.GetComponentInChildren<HomingJavelin>().SetTargetRpc(targetId); // Target other client
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateHorseRpc() 
    {
        horse.SetActive(true);

        if (!IsOwner) return;

        GetComponent<Horse>().enabled = true;

        inputHandler.BlockInput();
        onAnimatorCrossFade?.Invoke("horseriding", .25f);
    }

    [Rpc(SendTo.Everyone)]
    public void DeactivateHorseRpc(string nextAnimation, bool dead = false)
    {
        horse.SetActive(false);

        if (!IsOwner) return;
        if (dead) return;
        
        GetComponent<Horse>().enabled = false;

        inputHandler.UnblockInput();
        onAnimatorCrossFade?.Invoke(nextAnimation, .25f);
    }
    #endregion

    #region States
    [Rpc(SendTo.Everyone)]
    public void HitRpc() => CurrentState?.SwitchState(states?.Hit());
    [Rpc(SendTo.Everyone)]
    public void DeathRpc() => CurrentState?.SwitchState(states?.Death());
    #endregion

    #region UI Elements
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
    #endregion
    #endregion
}
