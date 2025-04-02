using UnityEngine;
using Unity.Netcode;

public class NetworkTester : NetworkBehaviour
{
    [SerializeField] GameObject npcPrefab;
    [SerializeField] Transform spawnPos;

    public void SpawnNPC()
    {
        if (!IsServer) return;

        var clone = Instantiate(npcPrefab, spawnPos.position, Quaternion.identity);
        clone.GetComponent<NetworkObject>().Spawn();
    }
}
