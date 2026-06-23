using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    //Weapon Settings
    public bool Type;
    public int Damage;
    public float Range;
    public float Rate;
    public float Recoil;
    public float Timer;
    public int Ammo;
    public int MaxAmmo;
    public float AmmoTimer;

    [SerializeField] private Transform weaponHolder;
    private GameObject weaponCurrent;

    //CameraController cameraCon;

    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;
    //PlayerInputHandler player = gameManager.instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
       // cameraCon = FindAnyObjectByType<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Equip(bool type, int damage, float range, float rate, float recoil, float timer, GameObject weaponPrefab, int ammo, int maxAmmo, float ammoTimer)
    {
        Type = type;
        Damage = damage;
        Range = range;
        Rate = rate;
        
        Recoil = recoil;
        gameManager.instance.recoil = recoil;


        Timer = timer;
        Ammo = ammo;
        MaxAmmo = maxAmmo;
        AmmoTimer = ammoTimer;

        if (weaponCurrent != null)
        {
            Destroy(weaponCurrent);
        }

        weaponCurrent = Instantiate(weaponPrefab, weaponHolder); 
        //you must place the weapon prefab corrisponding with the weapon pick-up prefab
        //this allows the player to obtain the weapon in their view
        //Only working prefab so far is pistol3, but we can absolutely add them all eventually

        weaponCurrent.transform.localPosition = Vector3.zero;
        weaponCurrent.transform.localRotation = Quaternion.identity;
    }
}

