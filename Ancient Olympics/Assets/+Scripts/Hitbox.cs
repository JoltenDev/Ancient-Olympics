using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (other.CompareTag("Player"))
            Debug.Log("Hit");
    }
}
