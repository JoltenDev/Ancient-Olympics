using System;
using System.Collections.Generic;
using Unity.Netcode;
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

    public Action onSwingStarted;
    public Action onSwingCompleted;

    [Rpc(SendTo.Everyone)]
    public void EquipWeaponRpc(ulong id, int index, bool hidden = false)
    {
        if (equippedWeapon != null) return;

        weaponData = weapons[index];
        if (weaponData == null) return;

        equippedWeapon = Instantiate(weaponData.weaponPrefab, hand);

        // Setup hitbox
        hitbox = equippedWeapon.GetComponentInChildren<Hitbox>();
        if (!hitbox.hitboxAlwaysActive)
            hitbox.gameObject.SetActive(false);
        hitbox.SetDamage(weaponData.damage, weaponData.knockbackStrength);

        // Assign events
        onSwingStarted += ActivateHitboxRpc;
        onSwingCompleted += DeactivateHitboxRpc;

        if (hidden)
            HideWeapon(id);
    }

    void HideWeapon(ulong id)
    {
        if (NetworkManager.LocalClientId == id) return;
        equippedWeapon.SetActive(false);
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
    void ActivateHitboxRpc()
    {
        if (!IsOwner) return;
        hitbox.gameObject.SetActive(true);
    }
    [Rpc(SendTo.Everyone)]
    void DeactivateHitboxRpc()
    {
        if (!IsOwner) return;

        if (!hitbox.hitboxAlwaysActive)
            hitbox.gameObject.SetActive(false);
    }
}
