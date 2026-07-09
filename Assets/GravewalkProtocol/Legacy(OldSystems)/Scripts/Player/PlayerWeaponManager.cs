using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour, IPickupAbilities
{
    [Header("Current Weapon Data")]
    [SerializeField] private WeaponData currentWeaponData;
    [SerializeField] private string currentWeaponName;
    [SerializeField] private List<string> currentWeaponMods = new List<string>();

    // Weapon Settings
    public bool Type;
    public int Damage;
    public float Range;
    public float Rate;
    public float Recoil;
    public float Timer;
    public float TimerOrig;
    public int Ammo;
    public int MaxAmmo;
    public float AmmoTimer;
    public BaseSoundSO ShootSound;
    public BaseSoundSO ReloadSound;
    public GameObject HitEffect;
    public Animator weaponAnimator;

    [SerializeField] private Transform weaponHolder;
    private GameObject weaponCurrent;

    // Ability Stuff
    [SerializeField] private GameObject abilityModel;
    public ParticleSystem effect;
    [SerializeField] public Transform effectSocket;
    private ParticleSystem activeEffect;
    [SerializeField] public List<AbilityStats> abilities = new List<AbilityStats>();
    private int firePos;
    private int freezePos;
    private int bouncePos;
    private int zoomPos;
    [SerializeField] public GameObject bouncePad;

    // Ability Settings
    public int fireLevel;
    public int freezeLevel;
    public int bounceLevel;
    public int zoomLevel;
    public int abilitySlot;

    // Animation hashes
    private int lightAttack = Animator.StringToHash("isHitting");
    private readonly int heavyAttack = Animator.StringToHash("heavyHit");

    [Header("Don't touch unless debugging")]
    [SerializeField] private List<string> Modifiers = new List<string>();

    public string CurrentWeaponName => currentWeaponName;
    public WeaponData CurrentWeaponData => currentWeaponData;
    public bool HasWeapon => currentWeaponData != null;

    void Update()
    {
        abilitySwitch();
    }

    public bool EquipPickedUpWeapon(
        WeaponData weaponData,
        int damage,
        float range,
        float rate,
        float recoil,
        float timer,
        int ammo,
        int maxAmmo,
        float ammoTimer,
        List<string> weaponMods = null
        )
    {
        if (weaponData == null)
        {
            return false;
        }

        currentWeaponData = weaponData;
        currentWeaponName = weaponData.weaponName;
        currentWeaponMods = CopyModList(weaponMods);

        Equip(
            weaponData.weaponType,
            damage,
            range,
            rate,
            recoil,
            timer,
            weaponData.weaponPrefab,
            ammo,
            maxAmmo,
            ammoTimer,
            weaponData.shootSound,
            weaponData.reloadSound,
            weaponData.hitEffect
        );

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Equipped " + currentWeaponName);
        }

        return true;
    }

    public string GetCurrentWeaponInfoText()
    {
        if (currentWeaponData == null)
        {
            return "Current Weapon\nNone";
        }

        string info = "";
        info += "Current Weapon\n";
        info += currentWeaponName + "\n\n";
        info += "DMG: " + Damage + "\n";
        info += "Ammo: " + Ammo + " / " + MaxAmmo + "\n";
        info += "Reload: " + AmmoTimer.ToString("0.00") + "s\n";
        info += "Fire Delay: " + Timer.ToString("0.00") + "s\n";
        info += "Range: " + Mathf.RoundToInt(Range) + "\n";

        info += "\nMods\n";

        if (currentWeaponMods == null || currentWeaponMods.Count == 0)
        {
            info += "No weapon mods";
        }
        else
        {
            for (int i = 0; i < currentWeaponMods.Count; i++)
            {
                info += currentWeaponMods[i] + "\n";
            }
        }

        return info;
    }

    private List<string> CopyModList(List<string> source)
    {
        if (source == null)
        {
            return new List<string>();
        }

        return new List<string>(source);
    }

    public void Equip(
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
        GameObject hitEffect)
    {
        Type = type;
        Damage = damage;
        Range = range;
        Rate = rate;

        Recoil = recoil;
        if (gameManager.instance != null)
        {
            gameManager.instance.recoil = recoil;
        }

        Timer = timer;
        TimerOrig = timer;
        Ammo = ammo;
        MaxAmmo = maxAmmo;
        AmmoTimer = ammoTimer;
        ShootSound = shootSound;
        ReloadSound = reloadSound;
        HitEffect = hitEffect;

        if (weaponCurrent != null)
        {
            Destroy(weaponCurrent);
        }

        if (weaponPrefab != null && weaponHolder != null)
        {
            weaponCurrent = Instantiate(weaponPrefab, weaponHolder);
            weaponCurrent.transform.localPosition = Vector3.zero;
            weaponCurrent.transform.localRotation = Quaternion.identity;

            weaponAnimator = weaponCurrent.GetComponent<Animator>();

            if (weaponAnimator != null)
            {
                weaponAnimator.SetBool("pickedUp", true);
            }
        }
    }

    public void getStats(AbilityStats stats)
    {
        switch (stats.abilityType)
        {
            case 1:
                if (fireLevel == 0)
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

        if (gameManager.instance != null)
        {
            gameManager.instance.abilityUI.abilityAssign(stats.abilityType);
            gameManager.instance.slotFiller();
        }
    }

    void abilityEquip(AbilityStats stats)
    {
        if (stats == null || stats.model == null || abilityModel == null)
        {
            return;
        }

        MeshFilter abilityMeshFilter = abilityModel.GetComponent<MeshFilter>();
        MeshFilter statsMeshFilter = stats.model.GetComponent<MeshFilter>();
        MeshRenderer abilityMeshRenderer = abilityModel.GetComponent<MeshRenderer>();
        MeshRenderer statsMeshRenderer = stats.model.GetComponent<MeshRenderer>();

        if (abilityMeshFilter != null && statsMeshFilter != null)
        {
            abilityMeshFilter.sharedMesh = statsMeshFilter.sharedMesh;
        }

        if (abilityMeshRenderer != null && statsMeshRenderer != null)
        {
            abilityMeshRenderer.sharedMaterial = statsMeshRenderer.sharedMaterial;
        }

        effect = stats.loopedEffect;

        if (activeEffect != null)
        {
            Destroy(activeEffect.gameObject);
        }

        if (effect != null && effectSocket != null)
        {
            activeEffect = Instantiate(effect, effectSocket);
            activeEffect.transform.localPosition = Vector3.zero;
            activeEffect.transform.localRotation = Quaternion.identity;
        }
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

    public void PlayMeleeLightAttack()
    {
        if (weaponAnimator == null)
            return;

        weaponAnimator.SetBool(lightAttack, true);
    }

    public void PlayMeleeHeavyAttack()
    {
        if (weaponAnimator == null)
            return;

        weaponAnimator.SetBool(heavyAttack, true);
    }

    public void ResetMeleeAnimationTriggers()
    {
        if (weaponAnimator == null)
            return;

        weaponAnimator.SetBool(lightAttack, false);
        weaponAnimator.SetBool(heavyAttack, false);
        gameManager.instance.isMeleeing = false;
    }
}