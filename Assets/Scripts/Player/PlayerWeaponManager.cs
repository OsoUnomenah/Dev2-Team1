using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour, IPickupAbilities
{
    [System.Serializable]
    public class InventoryWeapon
    {
        public string weaponName;
        public bool type;
        public int damage;
        public float range;
        public float rate;
        public float recoil;
        public float timer;
        public int ammo;
        public int maxAmmo;
        public float ammoTimer;
        public GameObject weaponPrefab;

    }

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
    
    //Ability Stuff
    [SerializeField] GameObject abilityModel;
    public ParticleSystem effect;
    [SerializeField] public Transform effectSocket;
    private ParticleSystem activeEffect;
    [SerializeField] public List<AbilityStats> abilities = new List<AbilityStats>();
    private int firePos;
    private int freezePos;
    private int bouncePos;
    private int zoomPos;
    //Ability Settings
    public int fireLevel;
    public int freezeLevel;
    public int bounceLevel;
    public int zoomLevel;
    public int abilitySlot;

    [Header("Weapon Inventory")]
    [SerializeField] private List<InventoryWeapon> weaponInventory = new List<InventoryWeapon>();
    [SerializeField] private int currentWeaponIndex = -1;
    [SerializeField] private int maxWeaponSlots = 4;

    public int CurrentWeaponIndex => currentWeaponIndex;
    public int WeaponCount => weaponInventory.Count;
    public int MaxWeaponSlots => maxWeaponSlots;


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
        HandleWeaponSwitchInput();
        abilitySwitch();
    }

    public bool AddWeaponToInventory(
    string weaponName,
    bool type,
    int damage,
    float range,
    float rate,
    float recoil,
    float timer,
    GameObject weaponPrefab,
    int ammo,
    int maxAmmo,
    float ammoTimer)
    {
        for (int i = 0; i < weaponInventory.Count; i++)
        {
            if (weaponInventory[i].weaponName == weaponName)
            {
                if (UpgradeUI.instance != null)
                {
                    UpgradeUI.instance.ShowUpgradeNotification("Already have " + weaponName);
                }

                Debug.Log("Player already has weapon: " + weaponName);
                return false;
            }
        }

        if (weaponInventory.Count >= maxWeaponSlots)
        {
            if (UpgradeUI.instance != null)
            {
                UpgradeUI.instance.ShowUpgradeNotification("Weapon inventory full");
            }

            Debug.Log("Weapon inventory full.");
            return false;
        }

        InventoryWeapon newWeapon = new InventoryWeapon();

        newWeapon.weaponName = weaponName;
        newWeapon.type = type;
        newWeapon.damage = damage;
        newWeapon.range = range;
        newWeapon.rate = rate;
        newWeapon.recoil = recoil;
        newWeapon.timer = timer;
        newWeapon.weaponPrefab = weaponPrefab;
        newWeapon.ammo = ammo;
        newWeapon.maxAmmo = maxAmmo;
        newWeapon.ammoTimer = ammoTimer;

        weaponInventory.Add(newWeapon);

        currentWeaponIndex = weaponInventory.Count - 1;

        // Do not save the previous weapon's ammo into this brand-new weapon slot.
        EquipWeaponFromInventory(currentWeaponIndex, false);

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Picked up " + weaponName);
        }

        Debug.Log("Added weapon to inventory: " + weaponName);
        return true;
    }

    private void SaveCurrentAmmoToInventory()
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= weaponInventory.Count)
        {
            return;
        }

        weaponInventory[currentWeaponIndex].ammo = Ammo;
    }

    private void EquipWeaponFromInventory(int index, bool saveCurrentAmmo = true)
    {
        if (index < 0 || index >= weaponInventory.Count)
        {
            return;
        }

        if (saveCurrentAmmo)
        {
            SaveCurrentAmmoToInventory();
        }

        currentWeaponIndex = index;

        InventoryWeapon weapon = weaponInventory[currentWeaponIndex];

        Equip(
            weapon.type,
            weapon.damage,
            weapon.range,
            weapon.rate,
            weapon.recoil,
            weapon.timer,
            weapon.weaponPrefab,
            weapon.ammo,
            weapon.maxAmmo,
            weapon.ammoTimer
        );

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Equipped " + weapon.weaponName);
        }
    }

    public string GetWeaponNameAtSlot(int index)
    {
        if (index < 0 || index >= weaponInventory.Count)
        {
            return "Empty";
        }

        return weaponInventory[index].weaponName;
    }

    private void SwitchWeapon(int direction)
    {
        if (weaponInventory.Count <= 1)
        {
            return;
        }

        int newIndex = currentWeaponIndex + direction;

        if (newIndex >= weaponInventory.Count)
        {
            newIndex = 0;
        }
        else if (newIndex < 0)
        {
            newIndex = weaponInventory.Count - 1;
        }

        EquipWeaponFromInventory(newIndex);
    }

    private void HandleWeaponSwitchInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            SwitchWeapon(1);
        }
        else if (scroll < 0f)
        {
            SwitchWeapon(-1);
        }                
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

    public void getStats(AbilityStats stats)
    {
        
        switch(stats.abilityType)
        {
            case 1:
                if(fireLevel == 0)
                {
                    firstTimePickup(stats);
                    firePos = abilities.Count - 1;
                }
                abilityEquip(abilities[firePos]);
                fireLevel += stats.level;
                break;
            case 2:
                if (freezeLevel == 0)
                {
                    firstTimePickup(stats);
                    freezePos = abilities.Count - 1;
                }
                abilityEquip(abilities[freezePos]);
                freezeLevel += stats.level;
                break;
            case 3:
                if (bounceLevel == 0)
                {
                    firstTimePickup(stats);
                    bouncePos = abilities.Count - 1;
                }
                abilityEquip(abilities[bouncePos]);
                bounceLevel += stats.level;
                break;
            case 4:
                if (zoomLevel == 0)
                {
                    firstTimePickup(stats);
                    zoomPos = abilities.Count - 1;
                }
                abilityEquip(abilities[zoomPos]);
                zoomLevel += stats.level;
                break;
        }
       
    }

    void firstTimePickup(AbilityStats stats)
    {
        abilities.Add(stats);
        
        gameManager.instance.abilityUI.abilityAssign(stats.abilityType);

        gameManager.instance.slotFiller();
    }

    void abilityEquip(AbilityStats stats)
    {
        abilityModel.GetComponent<MeshFilter>().sharedMesh = stats.model.GetComponent<MeshFilter>().sharedMesh;
        abilityModel.GetComponent<MeshRenderer>().sharedMaterial = stats.model.GetComponent<MeshRenderer>().sharedMaterial;
        effect = stats.loopedEffect;

        if (activeEffect != null)
        {
            Destroy(activeEffect.gameObject);
        }
        effect = stats.loopedEffect;

        activeEffect = Instantiate(effect, effectSocket);
        activeEffect.transform.localPosition = Vector3.zero;
        activeEffect.transform.localRotation = Quaternion.identity;
    }
    void abilitySwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && abilities.Count > 0)
        {
            abilityEquip(abilities[0]);
            abilitySlot = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && abilities.Count > 1)
        {
            abilityEquip(abilities[1]);
            abilitySlot = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && abilities.Count > 2)
        {
            abilityEquip(abilities[2]);
            abilitySlot = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && abilities.Count > 3)
        {
            abilityEquip(abilities[3]);
            abilitySlot = 3;
        }
    }
}

