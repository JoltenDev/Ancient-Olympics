using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] Transform hand;
    [SerializeField] GameObject equippedWeapon;
    [SerializeField] List<Weapon> weapons = new List<Weapon>();
    public List<GameObject> projectiles = new List<GameObject>();

    Weapon weaponData;
    Hitbox hitbox;

    public GameObject EquippedWeapon { get => equippedWeapon; }
    public Weapon WeaponData { get => weaponData; }
    public Transform Hand { get => hand; }

    public static Action onSwingStarted;
    public static Action onSwingCompleted;

    [Rpc(SendTo.Everyone)]
    public void EquipWeaponClientRpc(int index)
    {
        weaponData = weapons[index];
        if (weaponData == null) return;

        equippedWeapon = Instantiate(weaponData.weaponPrefab, hand);

        // Setup hitbox
        hitbox = equippedWeapon.GetComponentInChildren<Hitbox>();
        hitbox?.SetOwner(OwnerClientId);
        hitbox.gameObject.SetActive(false);

        // Assign events
        onSwingStarted += ActivateHitboxRpc;
        onSwingCompleted += DeactivateHitboxRpc;
    }

    [Rpc(SendTo.Everyone)]
    public void UnequipWeaponRpc()
    {
        Destroy(equippedWeapon);
        
        weaponData = null;
        equippedWeapon = null;

        onSwingStarted -= ActivateHitboxRpc;
        onSwingCompleted -= DeactivateHitboxRpc;
    }

    [Rpc(SendTo.Everyone)]
    void ActivateHitboxRpc() => hitbox?.gameObject.SetActive(true);
    [Rpc(SendTo.Everyone)]
    void DeactivateHitboxRpc() => hitbox?.gameObject.SetActive(false);
}
