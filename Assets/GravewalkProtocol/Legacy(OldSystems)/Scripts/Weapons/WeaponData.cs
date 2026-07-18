using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;

    [Header("Weapon Type")]
    public bool weaponType; // true = melee, false = projectile

    [Header("Firing")]
    public bool fullAuto;

    [Header("Shotgun")]
    [Tooltip("Enable this weapon to fire multiple pellets per shot")]
    public bool usesPellets;

    [Min(1)]
    [Tooltip("Number of pellets fired in buckshot")]
    public int pelletCount = 8;

    [Min(0f)]
    [Tooltip("Max pellet spread to the left and right.")]
    public float horizontalSpread = 15f;

    [Min(0f)]
    [Tooltip("Max pellet spread up and down.")]
    public float verticalSpread = 1f;

    [Header("Charged Shot")]
    [Tooltip("Makes this weapon charge while the fire button is held.")]
    public bool usesChargedShot;

    [Min(0.1f)]
    [Tooltip("Time required to reach full charge.")]
    public float chargeTime = 2f;

    [Min(0f)]
    [Tooltip("Delay after firing before the weapon can charge again.")]
    public float chargeCooldown = 2f;

    [Min(1f)]
    [Tooltip("Damage multiplier at full charge before critical damage.")]
    public float maxChargeMultiplier = 2f;

    [Min(1f)]
    [Tooltip("Additional multiplier when fired at full charge.")]
    public float criticalMultiplier = 1.5f;

    [UnityEngine.Range(10f, 100f)]
    [Tooltip("Camera field of view while fully charged.")]
    public float chargedZoomFOV = 35f;

    [Header("Stats")]
    public int damage;
    public float range;
    public float rate;
    public float recoil;
    public float timer;
    public int ammo;
    public int maxAmmo;
    public float ammoTimer;
    public float ads;
    [Header("Visuals")]
    public GameObject weaponPrefab;
    public GameObject hitEffect;

    [Header("Audio")]
    public BaseSoundSO shootSound;
    public BaseSoundSO reloadSound;
    public BaseSoundSO weaponPickupSound;

}