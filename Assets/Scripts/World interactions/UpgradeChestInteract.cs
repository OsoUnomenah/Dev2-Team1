using UnityEngine;
using System.Collections;

public class UpgradeChestInteract : MonoBehaviour, IInteract
{
    private enum UpgradeType
    {
        Damage,
        Defense,
        MaxHealth,
        MaxStamina,
        Speed,
        Jumps
    }

    private enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Header("Chest Settings")]
    [SerializeField] private Transform lidTransform;
    [SerializeField] private float openAngle;
    [SerializeField] private float openSpeed;

    [Header("Enemy Trap Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] float enemySpawnChance;

    [Header("Highlight Settings")]
    [SerializeField] private Renderer model;
    [SerializeField] private Material highlight;

    private Material materialOrig;
    private bool isOpen;
    private bool isMoving;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private StatHandler playerStats;

    void Start()
    {
        if (lidTransform == null)
        {
            lidTransform = transform;
        }

        closedRotation = lidTransform.rotation;
        openRotation = closedRotation * Quaternion.Euler(openAngle, 0, 0);

        if (model != null)
        {
            materialOrig = model.material;
        }

        GameObject statObj = GameObject.FindGameObjectWithTag("PlayerStatHandler");

        if (statObj != null)
        {
            playerStats = statObj.GetComponent<StatHandler>();
        }

    }

    public void Interact()
    {

        if (isOpen || isMoving)
        {
            return;
        }

        StartCoroutine(OpenChest());
        isOpen = true;

    }

    private IEnumerator OpenChest()
    {
        isMoving = true;
        bool rewardGiven = false;

        while (Quaternion.Angle(lidTransform.rotation, openRotation) > 0.1f)
        {
            lidTransform.rotation = Quaternion.Slerp(
                lidTransform.rotation,
                openRotation,
                openSpeed * Time.deltaTime
            );

            //Give the upgrade while the chest is opening instead of waiting until it is fully open.
            if (!rewardGiven && Quaternion.Angle(lidTransform.rotation, closedRotation) > 20f)
            {
                GiveUpgradeReward();
                TrySpawnEnemy();
                rewardGiven = true;
            }

            yield return null;
        }

        lidTransform.rotation = openRotation;
        isMoving = false;

        Debug.Log("Upgrade chest opened!");
    }

    private void GiveUpgradeReward()
    {
        if(playerStats == null)
        {
            GameObject statObj = GameObject.FindGameObjectWithTag("PlayerStatHandler");

            if(statObj != null)
            {
                playerStats = statObj.GetComponent<StatHandler>();
            }
        }

        if(playerStats == null)
        {
            Debug.LogWarning("No StatHandler found. Upgrade chest could not give reward.");
            return;
        }

        UpgradeType upgrade = (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length);
        Rarity rarity = RollRarity();
        float amount = GetAmount(upgrade, rarity);

        ApplyUpgrade(upgrade, amount);

        if(UpgradeUI.instance != null)
        {
            UpgradeUI.instance.UpdateUpgradeText();
        }

        string rewardMessage = rarity + " " + upgrade + " +" + amount;

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.UpdateUpgradeText();
            UpgradeUI.instance.ShowUpgradeNotification(rewardMessage);
        }

        Debug.Log("Upgrade chest gave: " + rewardMessage);
    }

    private Rarity RollRarity()
    {
        int roll = Random.Range(1, 101);

        if (roll <= 60)
        {
            return Rarity.Common;
        }
        else if (roll <= 85)
        {
            return Rarity.Rare;
        }
        else if (roll <= 97)
        {
            return Rarity.Epic;
        }
        else
        {
            return Rarity.Legendary;
        }
    }

    private float GetAmount(UpgradeType upgrade, Rarity rarity)
    {
        switch (upgrade)
        {
            case UpgradeType.Damage:
                return GetRarityAmount(rarity, 2, 5, 10, 20);

            case UpgradeType.Defense:
                return GetRarityAmount(rarity, 1, 3, 6, 10);

            case UpgradeType.MaxHealth:
                return GetRarityAmount(rarity, 10, 20, 35, 50);

            case UpgradeType.MaxStamina:
                return GetRarityAmount(rarity, 10, 20, 35, 50);

            case UpgradeType.Speed:
                return GetRarityAmount(rarity, 1, 2, 3, 5);

            case UpgradeType.Jumps:
                return GetRarityAmount(rarity, 1, 1, 2, 3);
        }

        return 0;
    }

    private float GetRarityAmount(Rarity rarity, float common, float rare, float epic, float legendary)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return common;

            case Rarity.Rare:
                return rare;

            case Rarity.Epic:
                return epic;

            case Rarity.Legendary:
                return legendary;
        }

        return common;
    }

    private void ApplyUpgrade(UpgradeType upgrade, float amount)
    {
        switch (upgrade)
        {
            case UpgradeType.Damage:
                playerStats.modDamage += amount;
                break;

            case UpgradeType.Defense:
                playerStats.modDefense += amount;
                break;

            case UpgradeType.MaxHealth:
                playerStats.modHealth += amount;
                playerStats.maxHealth += amount;
                playerStats.currentHealth += amount;
                break;

            case UpgradeType.MaxStamina:
                playerStats.modStamina += amount;
                playerStats.maxStamina += amount;
                playerStats.currentStamina += amount;
                break;

            case UpgradeType.Speed:
                playerStats.modSpeed += amount;
                break;

            case UpgradeType.Jumps:
                playerStats.modJumps += amount;
                break;
        }
    }

    private void TrySpawnEnemy()
    {
        if (enemyPrefab == null || enemySpawnPoint == null)
            return;

        float roll = Random.Range(0f, 100f);

        if (roll <= enemySpawnChance)
        {
            Instantiate(enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
            Debug.Log("Unlucky Chest!!! Enemy spawned.");
        }
    }

    public void OnHoverEnter()
    {
        if (model != null && highlight != null)
        {
            model.material = highlight;
        }

        TempUI.OnHover(0);
    }

    public void OnHoverExit()
    {
        if (model != null && materialOrig != null)
        {
            model.material = materialOrig;
        }

        TempUI.OffHover();
    }
}
