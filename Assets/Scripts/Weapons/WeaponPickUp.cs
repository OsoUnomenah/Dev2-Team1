using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour, IInteract
{
    string objectName;

    [SerializeField] private string weaponName;
    
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

    [Header("Audio")]
    [SerializeField] private BaseSoundSO shootSound;
    [SerializeField] private BaseSoundSO reloadSound;
    [SerializeField] private BaseSoundSO weaponPickupSound;

    [Header("Generated Weapon Mods")]
    [SerializeField] private bool hasGeneratedMod;
    [SerializeField] private List<string> modDescriptions = new List<string>();

    private enum ModRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }


    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    private PlayerWeaponManager weaponManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materialOrig = model.material;
        weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        objectName = gameObject.name;
    }    

    public void RollChestWeaponMods()
    {
        if (hasGeneratedMod)
            return;

        hasGeneratedMod = true;
        modDescriptions.Clear();

        int modCount = Random.Range(1, 3);

        for(int i =0; i < modCount; i++)
        {
            ModRarity rarity = RollModRarity();
            float rarityMultiplier = GetRarityMultiplier(rarity);

            int roll = Random.Range(0, 5);

            switch (roll)
            {
                case 0:
                    int bonusDamage = Mathf.RoundToInt(Random.Range(3, 11) * rarityMultiplier);
                    damage += bonusDamage;
                    AddModDescription(rarity, "Damage +" + bonusDamage);
                    break;

                case 1:
                    int bonusAmmo = Mathf.RoundToInt(Random.Range(2, 8) * rarityMultiplier);
                    maxAmmo += bonusAmmo;
                    ammo = maxAmmo;
                    AddModDescription(rarity, "Max Ammo +" + bonusAmmo);
                    break;

                case 2:
                    float reloadBonus = Random.Range(0.15f, 0.35f) * rarityMultiplier;
                    ammoTimer = Mathf.Max(0.5f, ammoTimer - reloadBonus);
                    AddModDescription(rarity, "Reload Speed +" + Mathf.RoundToInt(reloadBonus * 100) + "%");
                    break;

                case 3:
                    float fireRateBonus = Random.Range(0.05f, 0.2f) * rarityMultiplier;
                    timer = Mathf.Max(0.05f, timer - fireRateBonus);
                    AddModDescription(rarity, "Fire Rate +" + Mathf.RoundToInt(fireRateBonus * 100) + "%");
                    break;

                case 4:
                    float rangeBonus = Random.Range(5f, 21f) * rarityMultiplier;
                    range += rangeBonus;
                    AddModDescription(rarity, "Range +" + Mathf.RoundToInt(rangeBonus));
                    break;
            }
        }

        //Debug.Log(gameObject.name + " rolled chest mods: " + string.Join(", ", modDescriptions));
    }

    private ModRarity RollModRarity()
    {
        int roll = Random.Range(1, 101);

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
                return 3f;
        }

        return 1f;
    }

    private string GetRarityColor(ModRarity rarity)
    {
        switch (rarity)
        {
            case ModRarity.Common:
                return "#B8B8B8"; // gray

            case ModRarity.Rare:
                return "#4DA6FF"; // blue

            case ModRarity.Epic:
                return "#B84DFF"; // purple

            case ModRarity.Legendary:
                return "#FFB84D"; // gold
        }

        return "#FFFFFF";
    }

    private void AddModDescription(ModRarity rarity, string description)
    {
        string color = GetRarityColor(rarity);
        modDescriptions.Add("<color=" + color + ">" + rarity + " " + description + "</color>");
    }

    public string GetWeaponHoverText()
    {
        string displayName = weaponName;

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = gameObject.name;
        }

        string info = "";

        info += "<b>" + displayName + "</b>\n";
        info += "DMG: " + damage + "\n";
        info += "Ammo: " + ammo + " / " + maxAmmo + "\n";
        info += "Reload: " + ammoTimer.ToString("0.00") + "s\n";
        info += "Fire Delay: " + timer.ToString("0.00") + "s\n";
        info += "Range: " + Mathf.RoundToInt(range) + "\n";

        info += "\n<b>Mods</b>\n";

        if (modDescriptions.Count == 0)
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
        //Debug.Log($"Picked up {objectName}");

        if (weaponManager == null)
        {
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
        }

        if (weaponManager == null)
        {
           // Debug.LogWarning("No PlayerWeaponManager found. Could not pick up weapon.");
            return;
        }

        bool pickedUp = weaponManager.AddWeaponToInventory(
    weaponName,
    weaponType,
    damage,
    range,
    rate,
    recoil,
    timer,
    weaponPrefab,
    ammo,
    maxAmmo,
    ammoTimer,
    shootSound,
    reloadSound,
    modDescriptions
);

        if (!pickedUp)
        {
            pickedUp = weaponManager.ReplaceWeaponInInventory(
                weaponName,
                weaponType,
                damage,
                range,
                rate,
                recoil,
                timer,
                weaponPrefab,
                ammo,
                maxAmmo,
                ammoTimer,
                shootSound,
                reloadSound,
                modDescriptions
            );
        }

        if (!pickedUp)
        {
            return;
        }

        gameManager.instance.interactText.gameObject.SetActive(false);
        RecticleBehaviour.OffHover();
        if(WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.HideInfo();
        }

        PlayWeaponPickupSound();

        Destroy(gameObject);
    }

    private void PlayWeaponPickupSound()
    {
        if (AudioManager.instance != null && weaponPickupSound != null)
        {
            AudioManager.instance.PlaySound(weaponPickupSound);
        }
    }

    public void OnHoverEnter()
    {
        model.material = highLight;
        gameManager.instance.interactText.gameObject.SetActive(true);
        RecticleBehaviour.OnHover(0);

        if(WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.ShowInfo(GetWeaponHoverText());
        }
    }

    public void OnHoverExit()
    {       
        model.material = materialOrig;
        gameManager.instance.interactText.gameObject.SetActive(false);
        RecticleBehaviour.OffHover();

        if(WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.HideInfo();
        }
    }
}
