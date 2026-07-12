using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopUI : MonoBehaviour
{
    public enum UpgradeType
    {
        AbilityChanceIncrease,
        AbilityDamageIncrease,
        DamageIncrease,
        DashStaminaCostDecrease,
        DefenseIncrease,
        MaxAmmoIncrease,
        MaxHealthIncrease,
        MaxStaminaIncrease,
        MovementSpeedIncrease,
        ReloadSpeedIncrease
    }

    [System.Serializable]
    public class ShopUpgrade
    {
        public UpgradeType type;
        public string displayName;
        public int currentCost = 10;
        public int costIncreasePerPurchase = 5;
        public TMP_Text costText;
        public Button buyButton;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private ShopUpgrade[] upgrades;
    [SerializeField] private GameObject shopCanvasRoot;

    public bool shopOpen;

    [Header("Player References")]
    [SerializeField] private StatHandler playerStats;
    [SerializeField] private PlayerWeaponManager weaponManager;
    [SerializeField] private UpgradeUI upgradeUI;

    [Header("Upgrade Amounts")]
    [SerializeField] private float abilityDamageIncreaseAmount = 1f;
    [SerializeField] private float damageIncreaseAmount = 1f;
    [SerializeField] private float dashCostDecreaseAmount = 1f;
    [SerializeField] private float defenseIncreaseAmount = 1f;
    [SerializeField] private int maxAmmoIncreaseAmount = 5;
    [SerializeField] private float maxHealthIncreaseAmount = 10f;
    [SerializeField] private float maxStaminaIncreaseAmount = 10f;
    [SerializeField] private float movementSpeedIncreaseAmount = 1f;
    [SerializeField] private float reloadSpeedDecreaseAmount = 0.1f;

    private void Start()
    {
        if (playerStats == null)
            playerStats = gameManager.instance.playerStatHandler;

        if (weaponManager == null)
            weaponManager = gameManager.instance.playerWeaponManager;

        if (upgradeUI == null)
            upgradeUI = UpgradeUI.instance;

        if (shopCanvasRoot != null)
            shopCanvasRoot.SetActive(false);

        shopOpen = false;

        RefreshShopUI();
    }

    public void BuyUpgrade(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= upgrades.Length)
            return;

        ShopUpgrade selectedUpgrade = upgrades[upgradeIndex];

        if (!gameManager.instance.SpendCurrency(selectedUpgrade.currentCost))
        {
            ShowFeedback("Not enough currency for " + selectedUpgrade.displayName);
            RefreshShopUI();
            return;
        }

        ApplyUpgrade(selectedUpgrade.type);

        selectedUpgrade.currentCost += selectedUpgrade.costIncreasePerPurchase;

        RefreshShopUI();

        if (upgradeUI != null)
        {
            upgradeUI.UpdateUpgradeText();
            upgradeUI.ShowUpgradeNotification(selectedUpgrade.displayName + " purchased");
        }

        ShowFeedback(selectedUpgrade.displayName + " purchased");

        UpgradeUI.instance.RefreshAllUI();
    }

    private void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.AbilityChanceIncrease:
                gameManager.instance.IncreaseAbilityProcChance();
                break;

            case UpgradeType.AbilityDamageIncrease:
                gameManager.instance.abilityDamageBonus += abilityDamageIncreaseAmount;
                break;

            case UpgradeType.DamageIncrease:
                playerStats.modDamage += damageIncreaseAmount;
                break;

            case UpgradeType.DashStaminaCostDecrease:
                playerStats.dashCost = Mathf.Max(1f, playerStats.dashCost - dashCostDecreaseAmount);
                break;

            case UpgradeType.DefenseIncrease:
                playerStats.modDefense += defenseIncreaseAmount;
                break;

            case UpgradeType.MaxAmmoIncrease:
                weaponManager.MaxAmmo += maxAmmoIncreaseAmount;
                weaponManager.Ammo = Mathf.Min(
                    weaponManager.Ammo + maxAmmoIncreaseAmount,
                    weaponManager.MaxAmmo
                );
                break;

            case UpgradeType.MaxHealthIncrease:
                playerStats.modHealth += maxHealthIncreaseAmount;
                playerStats.maxHealth = playerStats.health + playerStats.modHealth;
                playerStats.currentHealth = Mathf.Min(
                    playerStats.currentHealth + maxHealthIncreaseAmount,
                    playerStats.maxHealth
                );
                playerStats.GE_OnPlayerHealthChanged.Raise(this, gameManager.instance.playerStatHandler);
                break;

            case UpgradeType.MaxStaminaIncrease:
                playerStats.modStamina += maxStaminaIncreaseAmount;
                playerStats.maxStamina = playerStats.stamina + playerStats.modStamina;
                playerStats.currentStamina = Mathf.Min(
                    playerStats.currentStamina + maxStaminaIncreaseAmount,
                    playerStats.maxStamina
                );
                playerStats.GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);
                break;

            case UpgradeType.MovementSpeedIncrease:
                playerStats.modSpeed += movementSpeedIncreaseAmount;
                break;

            case UpgradeType.ReloadSpeedIncrease:
                playerStats.modReloadSpeed += reloadSpeedDecreaseAmount;

                if (weaponManager != null)
                {
                    weaponManager.AmmoTimer = Mathf.Max(
                        0.1f,
                        weaponManager.BaseAmmoTimer - playerStats.modReloadSpeed
                    );
                }

                if (upgradeUI != null)
                {
                    upgradeUI.RefreshAllUI();
                }
                break;
        }
    }

    public void RefreshShopUI()
    {
        if (currencyText != null)
        {
            currencyText.text = "Currency: " + gameManager.instance.CurrentCurrency;
        }

        for (int i = 0; i < upgrades.Length; i++)
        {
            if (upgrades[i].costText != null)
            {
                upgrades[i].costText.text =
                    upgrades[i].displayName + "\nCost: " + upgrades[i].currentCost;
            }

            if (upgrades[i].buyButton != null)
            {
                upgrades[i].buyButton.interactable =
                    gameManager.instance.CurrentCurrency >= upgrades[i].currentCost;
            }
        }
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    public void ToggleShop()
    {
        if (shopOpen)
            CloseShop();
        else
            OpenShop();
    }

    public void OpenShop()
    {
        if (shopCanvasRoot == null)
            return;

        shopCanvasRoot.SetActive(true);
        shopOpen = true;

        if (gameManager.instance != null)
            gameManager.instance.statePause();

        RefreshShopUI();

        if (UpgradeUI.instance != null)
            UpgradeUI.instance.RefreshAllUI();
    }

    public void CloseShop()
    {
        if (shopCanvasRoot == null)
            return;

        shopCanvasRoot.SetActive(false);
        shopOpen = false;

        if (gameManager.instance != null)
            gameManager.instance.stateUnpause();
    }

    public bool IsShopOpen()
    {
        return shopOpen;
    }
}