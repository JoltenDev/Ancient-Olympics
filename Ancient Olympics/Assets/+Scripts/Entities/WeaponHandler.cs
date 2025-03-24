using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] Transform hand;
    [SerializeField] GameObject equippedWeapon;
    [SerializeField] List<Weapon> weapons = new List<Weapon>();

    Weapon weaponData;
    Hitbox hitbox;

    // Store currently equipped weapon index
    NetworkVariable<int> equippedWeaponIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameObject EquippedWeapon { get => equippedWeapon; }
    public Weapon WeaponData { get => weaponData; }

    public static Action onSwingStarted;
    public static Action onSwingCompleted;

    public override void OnNetworkSpawn()
    {
        if (IsClient && equippedWeaponIndex.Value != -1)
        {
            EquipWeaponClientRpc(equippedWeaponIndex.Value);
        }

        equippedWeaponIndex.OnValueChanged += (oldValue, newValue) =>
        {
            if (!IsServer) // Clients react to weapon changes
            {
                EquipWeaponClientRpc(newValue);
            }
        };
    }

    [Rpc(SendTo.Server)]
    public void EquipWeaponServerRpc(int index)
    {
        if (!IsServer) return;

        equippedWeaponIndex.Value = index; // Sync with all clients
        EquipWeaponClientRpc(index);
    }

    [Rpc(SendTo.Everyone)]
    void EquipWeaponClientRpc(int index)
    {
        if (equippedWeapon != null)
        {
            UnequipWeaponRpc();
        }

        weaponData = weapons[index];
        if (weaponData == null) return;

        equippedWeapon = Instantiate(weaponData.weaponPrefab, hand);

        // Setup hitbox
        hitbox = equippedWeapon.GetComponentInChildren<Hitbox>();
        hitbox?.SetOwner(OwnerClientId);
        hitbox.gameObject.SetActive(false);

        // Assign events
        onSwingStarted += ActivateHitbox;
        onSwingCompleted += DeactivateHitbox;
    }

    [Rpc(SendTo.Everyone)]
    public void UnequipWeaponRpc()
    {
        if (equippedWeapon != null)
            Destroy(equippedWeapon);

        equippedWeaponIndex.Value = -1;
        weaponData = null;
        equippedWeapon = null;

        onSwingStarted -= ActivateHitbox;
        onSwingCompleted -= DeactivateHitbox;
    }

    void ActivateHitbox() => hitbox?.gameObject.SetActive(true);
    void DeactivateHitbox() => hitbox?.gameObject.SetActive(false);
}
