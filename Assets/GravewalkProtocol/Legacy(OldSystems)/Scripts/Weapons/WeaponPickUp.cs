using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour, IInteract
{
    [SerializeField] private WeaponData weaponData;

    [SerializeField] private Renderer model;
    private Material materialOrig;
    [SerializeField] private Material highLight;
    [SerializeField] private Animator animator;

    [Header("Generated Weapon Mods")]
    [SerializeField] private bool hasGeneratedMod;
    [SerializeField] private List<string> modDescriptions = new List<string>();

    [Header("Rolled Runtime Stats")]
    [SerializeField] private int rolledDamage;
    [SerializeField] private float rolledRange;
    [SerializeField] private float rolledRate;
    [SerializeField] private float rolledRecoil;
    [SerializeField] private float rolledTimer;
    [SerializeField] private float ads;
    [SerializeField] private int rolledAmmo;
    [SerializeField] private int rolledMaxAmmo;
    [SerializeField] private float rolledAmmoTimer;

    [Header("Don't touch unless debugging")]
    [SerializeField] private List<string> Modifiers = new List<string>();

    private PlayerWeaponManager weaponManager;

    private enum ModRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    void Start()
    {
        if (model != null)
        {
            materialOrig = model.material;
        }

        weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
        ResetRolledStatsFromData();
    }

    private void ResetRolledStatsFromData()
    {
        if (weaponData == null)
        {
            return;
        }

        rolledDamage = weaponData.damage;
        rolledRange = weaponData.range;
        rolledRate = weaponData.rate;
        rolledRecoil = weaponData.recoil;
        rolledTimer = weaponData.timer;
        rolledAmmo = weaponData.ammo;
        rolledMaxAmmo = weaponData.maxAmmo;
        rolledAmmoTimer = weaponData.ammoTimer;
        ads = weaponData.ads;
    }

    public void RollChestWeaponMods(int CountMin, int CountMax, int RarityMin, int RarityMax)
    {
        if (hasGeneratedMod || weaponData == null)
        {
            return;
        }

        hasGeneratedMod = true;
        modDescriptions.Clear();
        ResetRolledStatsFromData();

        int modCount = Random.Range(CountMin, CountMax); //1-3 noramly

        for (int i = 0; i < modCount; i++)
        {
            ModRarity rarity = RollModRarity(RarityMin, RarityMax);
            float rarityMultiplier = GetRarityMultiplier(rarity);

            int roll = Random.Range(0, 5);

            switch (roll)
            {
                case 0:
                    int bonusDamage = Mathf.RoundToInt(Random.Range(3, 11) * rarityMultiplier);
                    rolledDamage += bonusDamage;
                    AddModDescription(rarity, "Damage +" + bonusDamage);
                    break;

                case 1:
                    if (weaponData.weaponType)
                    {
                        break;
                    }

                    int bonusAmmo = Mathf.RoundToInt(Random.Range(2, 8) * rarityMultiplier);
                    rolledMaxAmmo += bonusAmmo;
                    rolledAmmo = rolledMaxAmmo;
                    AddModDescription(rarity, "Max Ammo +" + bonusAmmo);
                    break;

                case 2:
                    if (weaponData.weaponType)
                    {
                        break;
                    }

                    float reloadBonus = Random.Range(0.15f, 0.35f) * rarityMultiplier;
                    rolledAmmoTimer = Mathf.Max(0.5f, rolledAmmoTimer - reloadBonus);
                    AddModDescription(rarity, "Reload Speed +" + Mathf.RoundToInt(reloadBonus * 100) + "%");
                    break;

                case 3:
                    if (weaponData.weaponType)
                    {
                        break;
                    }

                    float fireRateBonus = Random.Range(0.05f, 0.2f) * rarityMultiplier;
                    rolledTimer = Mathf.Max(0.05f, rolledTimer - fireRateBonus);
                    AddModDescription(rarity, "Fire Rate +" + Mathf.RoundToInt(fireRateBonus * 100) + "%");
                    break;

                case 4:
                    if (weaponData.weaponType)
                    {
                        break;
                    }

                    float rangeBonus = Random.Range(5f, 21f) * rarityMultiplier;
                    rolledRange += rangeBonus;
                    AddModDescription(rarity, "Range +" + Mathf.RoundToInt(rangeBonus));
                    break;
            }
        }
    }

    private ModRarity RollModRarity(int min, int max)
    {
        int roll = Random.Range(min, max); //1-101 normally

        if (roll <= 60)
        {
            return ModRarity.Common;
        }
        else if (roll <= 85)
        {
            return ModRarity.Rare;
        }
        else if (roll <= 97)
        {
            return ModRarity.Epic;
        }
        else
        {
            return ModRarity.Legendary;
        }
    }

    private float GetRarityMultiplier(ModRarity rarity)
    {
        switch (rarity)
        {
            case ModRarity.Common:
                return 1f;
            case ModRarity.Rare:
                return 1.5f;
            case ModRarity.Epic:
                return 2f;
            case ModRarity.Legendary:
                return 4f;
        }

        return 1f;
    }

    private void AddModDescription(ModRarity rarity, string description)
    {
        modDescriptions.Add(rarity + " " + description);
    }

    public string GetWeaponHoverText()
    {
        if (weaponData == null)
        {
            return "Weapon\nMissing WeaponData";
        }

        string displayName = string.IsNullOrEmpty(weaponData.weaponName) ? gameObject.name : weaponData.weaponName;

        string info = "";
        info += displayName + "\n";
        info += "DMG: " + rolledDamage + "\n";
        info += "Ammo: " + rolledAmmo + " / " + rolledMaxAmmo + "\n";
        info += "Reload: " + rolledAmmoTimer.ToString("0.00") + "s\n";
        info += "Fire Delay: " + rolledTimer.ToString("0.00") + "s\n";
        info += "Range: " + Mathf.RoundToInt(rolledRange) + "\n";

        info += "\nMods\n";

        if (modDescriptions == null || modDescriptions.Count == 0)
        {
            info += "No chest mods";
        }
        else
        {
            for (int i = 0; i < modDescriptions.Count; i++)
            {
                info += modDescriptions[i] + "\n";
            }
        }

        return info;
    }

    public void Interact()
    {
        Debug.Log("Interact called on: " + gameObject.name);

        if (weaponManager == null)
        {
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
        }

        if (weaponManager == null)
        {
            Debug.LogWarning("Weapon pickup failed: no PlayerWeaponManager found.");
            return;
        }

        if (weaponData == null)
        {
            Debug.LogWarning("Weapon pickup failed: no WeaponData assigned on " + gameObject.name);
            return;
        }

        Debug.Log("Trying to equip: " + weaponData.weaponName);

        bool pickedUp = weaponManager.EquipPickedUpWeapon(
            weaponData,
            rolledDamage,
            rolledRange,
            rolledRate,
            rolledRecoil,
            rolledTimer,
            rolledAmmo,
            rolledMaxAmmo,
            rolledAmmoTimer,
            ads,
            modDescriptions
        );

        Debug.Log("EquipPickedUpWeapon result: " + pickedUp);

        if (!pickedUp)
        {
            return;
        }

        if (gameManager.instance != null && gameManager.instance.interactText != null)
        {
            gameManager.instance.interactText.gameObject.SetActive(false);
        }

        RecticleBehaviour.OffHover();
        OnHoverExit();

        if (WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.HideInfo();
        }

        PlayWeaponPickupSound();
        Destroy(gameObject);
    }

    private void PlayWeaponPickupSound()
    {
        if (AudioManager.instance != null && weaponData != null && weaponData.weaponPickupSound != null)
        {
            AudioManager.instance.PlaySound(weaponData.weaponPickupSound);
        }
    }

    public void OnHoverEnter()
    {
        if(weaponManager.CurrentWeaponData == null)
        { return; }
        if (weaponManager.CurrentWeaponData == weaponData)
            return;

        if (model != null && highLight != null)
        {
            model.material = highLight;
        }

        if (gameManager.instance != null && gameManager.instance.interactText != null)
        {
            gameManager.instance.interactText.gameObject.SetActive(true);
        }

        RecticleBehaviour.OnHover(0);

        if (WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.ShowInfo(GetWeaponHoverText());
        }
        if(gameManager.instance.playerWeaponManager.CurrentWeaponName != null)
        {
            gameManager.instance.currentWeaponModText.text = weaponManager.GetCurrentWeaponInfoText();
            gameManager.instance.currentWeaponUI.gameObject.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        if (model != null && materialOrig != null)
        {
            model.material = materialOrig;
        }

        if (gameManager.instance != null && gameManager.instance.interactText != null)
        {
            gameManager.instance.interactText.gameObject.SetActive(false);
        }

        RecticleBehaviour.OffHover();

        if (WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.HideInfo();
        }
        if (gameManager.instance.playerWeaponManager.CurrentWeaponName != null)
        {
            gameManager.instance.currentWeaponUI.gameObject.SetActive(false);
        }
    }
}