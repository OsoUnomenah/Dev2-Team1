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
        public BaseSoundSO shootSound;
        public BaseSoundSO reloadSound;
        public List<string> modDescriptions = new List<string>();

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
    public BaseSoundSO ShootSound;
    public BaseSoundSO ReloadSound;

    

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
    [SerializeField] public GameObject bouncePad;
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
    float ammoTimer,
    BaseSoundSO shootSound,
    BaseSoundSO reloadSound,
    List<string> weaponMods = null)
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
        newWeapon.shootSound = shootSound;
        newWeapon.reloadSound = reloadSound;
        newWeapon.modDescriptions = CopyModList(weaponMods);

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

    public bool ReplaceWeaponInInventory(
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
    float ammoTimer,
    BaseSoundSO shootSound,
    BaseSoundSO reloadSound,
    List<string> weaponMods = null)
    {
        for (int i = 0; i < weaponInventory.Count; i++)
        {
            if (weaponInventory[i].weaponName == weaponName)
            {
                weaponInventory[i].type = type;
                weaponInventory[i].damage = damage;
                weaponInventory[i].range = range;
                weaponInventory[i].rate = rate;
                weaponInventory[i].recoil = recoil;
                weaponInventory[i].timer = timer;
                weaponInventory[i].weaponPrefab = weaponPrefab;
                weaponInventory[i].ammo = ammo;
                weaponInventory[i].maxAmmo = maxAmmo;
                weaponInventory[i].ammoTimer = ammoTimer;
                weaponInventory[i].shootSound = shootSound;
                weaponInventory[i].reloadSound = reloadSound;
                weaponInventory[i].modDescriptions = CopyModList(weaponMods);

                EquipWeaponFromInventory(i, false);

                if (UpgradeUI.instance != null)
                {
                    UpgradeUI.instance.ShowUpgradeNotification("Swapped " + weaponName);
                }

                Debug.Log("Replaced weapon in inventory: " + weaponName);
                return true;
            }
        }

        return false;
    }

    public string GetCurrentWeaponInfoText()
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= weaponInventory.Count)
        {
            return "<b>Current Weapon</b>\nNone";
        }

        InventoryWeapon weapon = weaponInventory[currentWeaponIndex];

        string info = "";

        info += "<b>Current Weapon</b>\n";
        info += "<color=#FFD966><b>" + weapon.weaponName + "</b></color>\n\n";

        info += "DMG: " + Damage + "\n";
        info += "Ammo: " + Ammo + " / " + MaxAmmo + "\n";
        info += "Reload: " + AmmoTimer.ToString("0.00") + "s\n";
        info += "Fire Delay: " + Timer.ToString("0.00") + "s\n";
        info += "Range: " + Mathf.RoundToInt(Range) + "\n";

        info += "\n<b>Mods</b>\n";

        if (weapon.modDescriptions == null || weapon.modDescriptions.Count == 0)
        {
            info += "No weapon mods";
        }
        else
        {
            for (int i = 0; i < weapon.modDescriptions.Count; i++)
            {
                info += weapon.modDescriptions[i] + "\n";
            }
        }

        return info;
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
            weapon.ammoTimer,
            weapon.shootSound,
            weapon.reloadSound
        );

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Equipped " + weapon.weaponName);
        }
    }

    private List<string> CopyModList(List<string> source)
    {
        if(source == null)
        {
            return new List<string>();
        }

        return new List<string>(source);
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
    public void Equip(bool type, int damage, float range, float rate, float recoil, float timer, GameObject weaponPrefab, int ammo, int maxAmmo, float ammoTimer, BaseSoundSO shootSound, BaseSoundSO reloadSound)
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
        ShootSound = shootSound;
        ReloadSound = reloadSound;

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

