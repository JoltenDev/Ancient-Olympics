using Unity.Netcode;
using UnityEngine;

public class HomingJavelin : NetworkBehaviour
{
    public Transform target;
    public float speed = 10f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // Ensure only the owner moves it
    }

    void Update()
    {
        if (!IsOwner || target == null) return;

        TrackTarget();
    }

    public void FindTarget()
    {
        if (!IsOwner) return; // Only the owner should set the target

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Player"))
            {
                target = hit.collider.transform;
                transform.parent = null;
                TrackTarget();
            }
        }
    }

    void TrackTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
