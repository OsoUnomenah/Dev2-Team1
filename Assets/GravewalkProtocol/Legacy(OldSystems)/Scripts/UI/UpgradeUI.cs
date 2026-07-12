using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI instance;

    [Header("Main Text")]
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private TMP_Text notifyText;
    [SerializeField] private TMP_Text currencyText;

    [Header("Optional Extra Text")]
    [SerializeField] private TMP_Text abilityStatsText;
    [SerializeField] private TMP_Text weaponStatsText;

    [Header("References")]
    [SerializeField] private StatHandler playerStats;
    [SerializeField] private PlayerWeaponManager weaponManager;

    private Coroutine notifyRoutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (playerStats == null && gameManager.instance != null)
        {
            playerStats = gameManager.instance.playerStatHandler;
        }

        if (weaponManager == null && gameManager.instance != null)
        {
            weaponManager = gameManager.instance.playerWeaponManager;
        }

        RefreshAllUI();

        if (notifyText != null)
        {
            notifyText.gameObject.SetActive(false);
        }
    }

    public void RefreshAllUI()
    {
        if (gameManager.instance != null)
        {
            playerStats = gameManager.instance.playerStatHandler;
            weaponManager = gameManager.instance.playerWeaponManager;
        }

        UpdateUpgradeText();
        UpdateCurrencyText();
        UpdateAbilityStatsText();
        UpdateWeaponStatsText();
    }

    public void UpdateUpgradeText()
    {
        if (upgradeText == null || playerStats == null)
            return;

        if (weaponManager == null && gameManager.instance != null)
        {
            weaponManager = gameManager.instance.playerWeaponManager;
        }

        string reloadText = "Reload Timer: No Weapon";

        if (weaponManager != null && weaponManager.HasWeapon)
        {
            reloadText = "Reload Timer: " + weaponManager.AmmoTimer.ToString("0.00") + "s";
        }

        upgradeText.text =
            "DMG +" + playerStats.modDamage + "\n" +
            "DEF +" + playerStats.modDefense + "\n" +
            "HP +" + playerStats.modHealth + "\n" +
            "STM +" + playerStats.modStamina + "\n" +
            "SPD +" + playerStats.modSpeed + "\n" +
            "JMP +" + playerStats.modJumps + "\n" +
            reloadText;
    }

    public void UpdateCurrencyText()
    {
        if (currencyText == null || gameManager.instance == null)
            return;

        currencyText.text = "Currency: " + gameManager.instance.CurrentCurrency;
    }

    public void UpdateAbilityStatsText()
    {
        if (abilityStatsText == null || gameManager.instance == null)
            return;

        abilityStatsText.text =
            "Ability Stats\n" +
            "Proc Chance: " + gameManager.instance.GetAbilityProcChancePercent().ToString("0") + "%\n" +
            "Ability DMG Bonus: +" + gameManager.instance.abilityDamageBonus;
    }

    public void UpdateWeaponStatsText()
    {
        if (weaponStatsText == null)
            return;

        if (weaponManager == null && gameManager.instance != null)
        {
            weaponManager = gameManager.instance.playerWeaponManager;
        }

        if (weaponManager == null || !weaponManager.HasWeapon)
        {
            weaponStatsText.text = "Weapon Stats\nNo Weapon Equipped";
            return;
        }

        weaponStatsText.text =
            "Weapon Stats\n" +
            "DMG: " + weaponManager.Damage + "\n" +
            "Ammo: " + weaponManager.Ammo + " / " + weaponManager.MaxAmmo + "\n" +
            "Reload Time: " + weaponManager.AmmoTimer.ToString("0.00") + "s";
    }

    public void ShowUpgradeNotification(string message)
    {
        if (notifyText == null)
            return;

        if (notifyRoutine != null)
        {
            StopCoroutine(notifyRoutine);
        }

        notifyRoutine = StartCoroutine(ShowNotificationRoutine(message));
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        notifyText.text = message;
        notifyText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        notifyText.gameObject.SetActive(false);
    }
}