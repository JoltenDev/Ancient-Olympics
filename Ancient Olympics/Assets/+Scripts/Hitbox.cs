using Unity.Netcode;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (other.gameObject != this && other.CompareTag("Player"))
        {
            Debug.Log("Hit");
            gameObject.SetActive(false);
        }    
    }
}
