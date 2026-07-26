using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour, IPickupAbilities
{
    [Header("Current Weapon Data")]
    [SerializeField] private WeaponData currentWeaponData;
    [SerializeField] private string currentWeaponName;
    [SerializeField] private List<string> currentWeaponMods = new List<string>();
    [SerializeField] Collider weaponCollider;

    [Header("Shotgun Settings")]
    public bool UsesPellets;
    public int PelletCount = 1;
    public float HorizontalSpread;
    public float VerticalSpread;

    [Header("Charged Shot Settings")]
    public bool UsesChargedShot;
    public float ChargeTime;
    public float ChargeCooldown;
    public float MaxChargeMultiplier;
    public float CriticalMultiplier;
    public float ChargedZoomFOV;

    // Weapon Settings
    public bool Type;
    public bool FullAuto;
    public int Damage;
    public float Range;
    public float Rate;
    public float Recoil;
    public float Timer;
    public float TimerOrig;
    public int Ammo;
    public int MaxAmmo;

    [Header("Reserve Ammo")]
    public int ReserveAmmo;
    public int MaxReserveAmmo;

    public float BaseAmmoTimer;
    public float AmmoTimer;
    public float Ads;
    public BaseSoundSO ShootSound;
    public BaseSoundSO ReloadSound;
    public GameObject HitEffect;
    public Animator playerAnimator;


    [SerializeField] public Transform weaponHolder;
    [SerializeField] public Transform adsWeaponHolder;
    [SerializeField] public Transform nonADSWeaponHolder;

    [Header("Steve Arm Visibility")]
    [SerializeField] private SkinnedMeshRenderer steveBodyRenderer;
    [SerializeField] private SkinnedMeshRenderer steveGlovesRenderer;
    [SerializeField] private SkinnedMeshRenderer steveTopsRenderer;

    private GameObject weaponCurrent;

    private Animator currentWeaponAnimator;

    public Animator CurrentWeaponAnimator => currentWeaponAnimator;

    // Ability Stuff
    [SerializeField] private GameObject abilityModel;
    public ParticleSystem effect;
    [SerializeField] public Transform effectSocket;
    private ParticleSystem activeEffect;
    [SerializeField] public List<AbilityStats> abilities = new List<AbilityStats>();
    private int firePos;
    private int freezePos;
    private int magnetPos;
    private int toxicPos;
    private int crystalPos = -100;
    private int lightningPos;
    [SerializeField] public GameObject bouncePad;
    [SerializeField] public SphereCollider magnetField;
    public ParticleSystem crystalHit;

    // Ability Settings
    public int fireLevel;
    public int freezeLevel;
    public int magnetLevel;
    public int toxicLevel;
    public int crystalLevel;
    public int lightningLevel;
    public int abilitySlot;

    [Header("Don't touch unless debugging")]
    [SerializeField] private List<string> Modifiers = new List<string>();

    public string CurrentWeaponName => currentWeaponName;
    public WeaponData CurrentWeaponData => currentWeaponData;
    public bool HasWeapon => currentWeaponData != null;

    void Start()
    {
        playerAnimator = gameManager.instance.playerAnimator.animator;
        gameManager.instance.crystalBarUI.SetActive(false);
    }

    void Update()
    {
        //abilitySwitch();
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
        float ads,

        List<string> weaponMods = null
        )
    {
        if (weaponData == null)
        {
            return false;
        }

        currentWeaponData = weaponData;
        FullAuto = weaponData.fullAuto;
        currentWeaponName = weaponData.weaponName;
        currentWeaponMods = CopyModList(weaponMods);

        // Shotgun
        UsesPellets = weaponData.usesPellets;
        PelletCount = Mathf.Max(1, weaponData.pelletCount);
        HorizontalSpread = weaponData.horizontalSpread;
        VerticalSpread = weaponData.verticalSpread;

        // Sniper 
        UsesChargedShot = weaponData.usesChargedShot;
        ChargeTime = weaponData.chargeTime;
        ChargeCooldown = weaponData.chargeCooldown;
        MaxChargeMultiplier = weaponData.maxChargeMultiplier;
        CriticalMultiplier = weaponData.criticalMultiplier;
        ChargedZoomFOV = weaponData.chargedZoomFOV;

        FullAuto = weaponData.fullAuto;

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
            weaponData.hitEffect,
            ads
        );

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Equipped " + currentWeaponName +
                " | Reload Timer: " + AmmoTimer.ToString("0.00") + "s" //kw
);
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
        info += "Reload Timer: " + AmmoTimer.ToString("0.00") + "s\n"; //kw
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
        GameObject hitEffect,
        float ads)
    {
        Type = type;
        UpdateSteveArmVisibility();
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

        // Ammo and MaxAmmo now represent the current magazine.
        if (!Type && currentWeaponData != null)
        {
            // Magazine values now come from the new WeaponData fields.
            MaxAmmo = Mathf.Max(1, currentWeaponData.magazineSize);
            Ammo = MaxAmmo;

            int bonusReserveAmmo = 0;

            if (gameManager.instance != null)
            {
                bonusReserveAmmo = gameManager.instance.BonusMaxAmmo;
            }

            MaxReserveAmmo = Mathf.Max(
                0,
                currentWeaponData.maxReserveAmmo + bonusReserveAmmo
            );

            ReserveAmmo = Mathf.Clamp(
                currentWeaponData.startingReserveAmmo,
                0,
                MaxReserveAmmo
            );
        }
        else
        {
            Ammo = 0;
            MaxAmmo = 0;
            ReserveAmmo = 0;
            MaxReserveAmmo = 0;
        }

        BaseAmmoTimer = ammoTimer;

        float reloadBonus = 0f;
        if (gameManager.instance != null && gameManager.instance.playerStatHandler != null)
        {
            reloadBonus = gameManager.instance.playerStatHandler.modReloadSpeed;
        }

        AmmoTimer = Mathf.Max(0.1f, BaseAmmoTimer - reloadBonus); //kw End here

        ShootSound = shootSound;
        ReloadSound = reloadSound;
        HitEffect = hitEffect;
        Ads = ads;

        ClearHeldWeaponViewmodels();

        if (weaponPrefab != null && weaponHolder != null)
        {
            weaponCurrent = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);

            currentWeaponAnimator =
    weaponCurrent.GetComponentInChildren<Animator>();

            weaponCollider = weaponCurrent.GetComponentInChildren<Collider>();
            weaponCollider.enabled = false;

            if (abilities != null && abilitySlot >= 0 && abilitySlot < abilities.Count)
            {
                ApplyAbilityEffectToCurrentWeapon(abilities[abilitySlot]);
            }
        }

        if (UpgradeUI.instance != null) //kw
        {
            UpgradeUI.instance.RefreshAllUI();
        }


    }

    public void getStats(AbilityStats stats)
    {
        switch (stats.abilityType)
        {
            case AbilityStats.ability.fire:
                if (fireLevel == 0)
                {
                    firstTimePickup(stats);
                    firePos = abilities.Count - 1;
                }
                abilityEquip(abilities[firePos]);
                fireLevel += stats.level;
                break;

            case AbilityStats.ability.freeze:
                if (freezeLevel == 0)
                {
                    firstTimePickup(stats);
                    freezePos = abilities.Count - 1;
                }
                abilityEquip(abilities[freezePos]);
                freezeLevel += stats.level;
                break;

            case AbilityStats.ability.magent:
                if (magnetLevel == 0)
                {
                    firstTimePickup(stats);
                    magnetPos = abilities.Count - 1;
                }
                abilityEquip(abilities[magnetPos]);
                magnetLevel += stats.level;
                break;

            case AbilityStats.ability.toxic:
                if (toxicLevel == 0)
                {
                    firstTimePickup(stats);
                    toxicPos = abilities.Count - 1;
                }
                abilityEquip(abilities[toxicPos]);
                toxicLevel += stats.level;
                break;
            case AbilityStats.ability.crystal:
                if (crystalLevel == 0)
                {
                    firstTimePickup(stats);
                    crystalPos = abilities.Count - 1;
                }
                abilityEquip(abilities[crystalPos]);
                crystalLevel += stats.level;
                break;
            case AbilityStats.ability.lightning:
                if (lightningLevel == 0)
                {
                    firstTimePickup(stats);
                    lightningPos = abilities.Count - 1;
                }
                abilityEquip(abilities[lightningPos]);
                lightningLevel += stats.level;
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
        if (stats == null)
        {
            return;
        }

        // Do not show the ability orb/model in the player's hand.
        if (abilityModel != null)
        {
            abilityModel.SetActive(false);
        }



        ApplyAbilityEffectToCurrentWeapon(stats);
    }

    private void ApplyAbilityEffectToCurrentWeapon(AbilityStats stats)
    {
        if (activeEffect != null)
        {
            Destroy(activeEffect.gameObject);
            activeEffect = null;
        }

        if (stats == null || stats.loopedEffect == null)
        {
            return;
        }

        if (weaponCurrent == null)
        {
            return;
        }

        effect = stats.loopedEffect;

        Transform effectParent = weaponCurrent.transform;

        Transform weaponEffectSocket = weaponCurrent.transform.Find("EffectSocket");

        if (weaponEffectSocket != null)
        {
            effectParent = weaponEffectSocket;
        }

        activeEffect = Instantiate(effect, effectParent);
        activeEffect.transform.localPosition = Vector3.zero;
        activeEffect.transform.localRotation = Quaternion.identity;
        activeEffect.Play();


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

        if (abilities.Count <= 0)
        {
            return;
        }

        if (abilities[abilitySlot].abilityName == "Crystal")
        {
            gameManager.instance.crystalBarUI.SetActive(true);
        }
        else
        {
            gameManager.instance.crystalBarUI.SetActive(false);
        }
    }

    public void PlayCurrentWeaponAnimation(string stateName)
    {
        if (currentWeaponAnimator == null ||
            !currentWeaponAnimator.isActiveAndEnabled ||
            string.IsNullOrEmpty(stateName))
        {
            return;
        }

        currentWeaponAnimator.CrossFadeInFixedTime(
            stateName,
            0.05f,
            0,
            0f
        );
    }

    private void UpdateSteveArmVisibility()
    {
        // Type is true for melee weapons and false for firearms.
        bool showSteveArms = Type;

        if (steveBodyRenderer != null)
        {
            steveBodyRenderer.enabled = showSteveArms;
        }

        if (steveGlovesRenderer != null)
        {
            steveGlovesRenderer.enabled = showSteveArms;
        }

        if (steveTopsRenderer != null)
        {
            steveTopsRenderer.enabled = showSteveArms;
        }
    }

    private void ClearHeldWeaponViewmodels()
    {
        currentWeaponAnimator = null;
        activeEffect = null;
        weaponCurrent = null;

        if (weaponHolder == null)
        {
            return;
        }

        for (int i = weaponHolder.childCount - 1; i >= 0; i--)
        {
            GameObject oldViewmodel =
                weaponHolder.GetChild(i).gameObject;

            // Hide it immediately because Destroy occurs at the end of the frame.
            oldViewmodel.SetActive(false);
            Destroy(oldViewmodel);
        }
    }
}