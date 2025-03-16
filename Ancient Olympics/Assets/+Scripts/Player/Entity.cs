using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TMPro;

public class Entity : NetworkBehaviour
{
    [SerializeField] float speed = 1f;
    Rigidbody rigidBody;
    Animator animator;
    Controls controls;

    Vector2 moveInput;

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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 targetPosition = hit.point;
            RotateRpc(targetPosition);
        }

        AnimateRpc();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        MoveRpc();
    }

    [Rpc(SendTo.Server)]
    void MoveRpc()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
        rigidBody.position += moveDir * speed * Time.fixedDeltaTime;
    }

    [Rpc(SendTo.Server)]
    void RotateRpc(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, 0.1f));
        }
    }

    [Rpc(SendTo.Server)]
    void AnimateRpc()
    {
        if (moveInput != Vector2.zero)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
    }
}
