using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;

    [Header("Weapon Type")]
    public bool weaponType; // true = melee, false = projectile

    [Header("Firing")]
    public bool fullAuto;

    [Header("Stats")]
    public int damage;
    public float range;
    public float rate;
    public float recoil;
    public float timer;
    public int ammo;
    public int maxAmmo;
    public float ammoTimer;

    [Header("Visuals")]
    public GameObject weaponPrefab;
    public GameObject hitEffect;

    [Header("Audio")]
    public BaseSoundSO shootSound;
    public BaseSoundSO reloadSound;
    public BaseSoundSO weaponPickupSound;
}