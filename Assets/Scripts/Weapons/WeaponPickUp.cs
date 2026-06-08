using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour, IInteract
{
    string objectName;
    
    [SerializeField] Renderer model;
    Material materialOrig;
    [SerializeField] Material highLight;
   
    [SerializeField] bool weaponType; //true for melee false for projectile
    [SerializeField] int damage;
    [SerializeField] float range;
    [SerializeField] float rate;
    [SerializeField] float recoil;
    [SerializeField] float timer;
    [SerializeField] public int ammo;
    [SerializeField] public int maxAmmo;
    [SerializeField] float ammoTimer;
    [SerializeField] private GameObject weaponPrefab;


    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    private PlayerWeaponManager weaponManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materialOrig = model.material;
        weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        objectName = gameObject.name;
    }    

    public void Interact()
    { 
        Debug.Log($"Picked up {objectName}");        

        weaponManager.Equip(weaponType, damage, range, rate, recoil, timer, weaponPrefab, ammo, maxAmmo, ammoTimer);

        TempUI.OffHover();
        Destroy(gameObject);
    }

    public void OnHoverEnter()
    {
        model.material = highLight;
        TempUI.OnHover(0);
    }

    public void OnHoverExit()
    {       
        model.material = materialOrig;
        TempUI.OffHover();
    }
}
