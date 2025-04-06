using UnityEngine;
using Unity.Netcode;

public class NetworkTester : NetworkBehaviour
{
    [SerializeField] GameObject npcPrefab;
    [SerializeField] Transform spawnPos;

    bool horseActivated = false;

    public void SpawnNPC()
    {
        if (!IsServer) return;

        var clone = Instantiate(npcPrefab, spawnPos.position, Quaternion.identity);
        clone.GetComponent<NetworkObject>().Spawn();
    }

    public void SpawnHorses()
    {
        if (!IsServer) return;

        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var client = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;

            if (!horseActivated)
            {
                client.GetComponent<Player>().WeaponHandler.EquipWeaponRpc(id, 3);
                client.GetComponent<Player>().ActivateHorseRpc();
            }
            else
            {
                client.GetComponent<Player>().WeaponHandler.UnequipWeaponRpc();
                client.GetComponent<Player>().DeactivateHorseRpc("idle");
            }
        }

        horseActivated = !horseActivated;
    }
}
