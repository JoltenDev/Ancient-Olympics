using UnityEngine;

public class Horse : NetworkEntity
{
    [Header("Horse Fields")]
    [SerializeField] float targetSpeed = 10f;
    [SerializeField] float acceleration = .8f;

    float currentSpeed;

    Vector3 moveDir;

    void OnEnable()
    {
        GetComponentInParent<Player>().onAnimatorSetBool("IsMounted", true);
    }

    void OnDisable()
    {
        GetComponentInParent<Player>().onAnimatorSetBool("IsMounted", false);
    }

    private void Update()
    {
        Rotate();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // Smooth speed transition
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        moveDir = transform.forward * currentSpeed;

        // Send to server: movement vector and current position
        SendMove(moveDir, transform.position);
    }

    void Rotate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 targetDir = (hit.point - transform.position).normalized;
            targetDir.y = 0f;

            if (targetDir != Vector3.zero)
            {
                // Smooth the current forward direction toward the target direction
                Vector3 smoothDirection = Vector3.Slerp(transform.forward, targetDir, Time.deltaTime * rotationSpeed);

                // Send smoothed direction
                SendRotation(smoothDirection);
            }
        }
    }
}
