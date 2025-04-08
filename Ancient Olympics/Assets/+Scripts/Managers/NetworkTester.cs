using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class NetworkTester : NetworkBehaviour
{
    [SerializeField] NetworkObject npc;
    [SerializeField] Transform spawnPos;

    [SerializeField] TMP_Text options_text;
    [SerializeField] List<GameObject> buttons = new List<GameObject>();

    [SerializeField] TMP_InputField weaponInputField;

    List<NetworkObject> spawnedNpcs = new List<NetworkObject>();

    bool optionsActivated = false;
    bool horseActivated = false;
    bool hudActivated = false;

    public void HandleOptions()
    {
        foreach (var button in buttons)
        {
            if (!optionsActivated)
            {
                options_text.text = "Close";
                button.SetActive(true);
            }
            else
            {
                options_text.text = "Options";
                button.SetActive(false);
            }
        }

        optionsActivated = !optionsActivated;
    }

    public void SpawnNPC()
    {
        if (!IsServer) return;

        var clone = npc.InstantiateAndSpawn(NetworkManager.Singleton, 120, true, false, false, new Vector3(0, 2f, 0), Quaternion.identity);
        spawnedNpcs.Add(clone);
    }

    public void DespawnAllNpcs()
    {
        foreach (var clone in spawnedNpcs)
        {
            clone.Despawn(true);
        }

        spawnedNpcs.Clear();
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
                client.GetComponent<Player>().DeactivateHorseRpc();
            }
        }

        horseActivated = !horseActivated;
    }

    public void RevivePlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            var health = client.PlayerObject.GetComponentInChildren<NetworkHealth>();
            health.SendHealRpc(health.MaxHealth);
        }
    }

    public void EquipWeapons()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var client = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
            client.GetComponent<Player>().WeaponHandler.EquipWeaponRpc(id, int.Parse(weaponInputField.text));
        }
    }

    public void UnequipWeapons() 
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var client = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
            client.GetComponent<Player>().WeaponHandler.UnequipWeaponRpc();
        }
    }

    public void TurnOnHud()
    {
        ToggleHudRpc(!hudActivated);
        hudActivated = !hudActivated;
    }

    [Rpc(SendTo.Everyone)]
    public void ToggleHudRpc(bool activate)
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject?.GetComponent<Player>();
        if (player != null && player.Hud != null)
        {
            player.Hud.SetActive(activate);
        }
    }
}
