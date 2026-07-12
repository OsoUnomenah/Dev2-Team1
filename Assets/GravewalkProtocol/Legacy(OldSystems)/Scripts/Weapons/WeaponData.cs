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