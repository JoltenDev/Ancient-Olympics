using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public GameObject weaponPrefab;  // Prefab of the weapon
    public Sprite weaponIcon; // UI Display
    public AttackType attackType; // Enum defining attack style

    public Hitbox GetHitbox() => weaponPrefab.GetComponentInChildren<Hitbox>();
}

public enum AttackType
{
    MeleeSword,
    MeleeKnife,
    Throw,
}
