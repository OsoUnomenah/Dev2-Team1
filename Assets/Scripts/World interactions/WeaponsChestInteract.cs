using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

public class WeaponsChestInteract : MonoBehaviour, IInteract
{
    [System.Serializable]
    public class WeaponReward
    {
        public string weaponName;
        public bool weaponType;
        public int damage;
        public float range;
        public float rate;
        public float recoil;
        public float timer;
        public int ammo;
        public int maxAmmo;
        public float ammoTimer;
        public GameObject weaponPrefab;
        public GameObject pickupPrefab;
    }

    [Header("Audio")]
    [SerializeField] private BaseSoundSO chestOpenSound;

    [Header("Chest Settings")]
    [SerializeField] private Transform lidTransform;
    [SerializeField] private float openAngle;
    [SerializeField] private float openSpeed;

    [Header("weapon Rewards")]
    [SerializeField] private WeaponReward[] weaponRewards;
    [SerializeField] private bool canGiveMaxAmmo = true;

    [Header("Enemy Trap Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnPoint;
    [Range(0f, 100f)] [SerializeField] float enemySpawnChance;

    [Header("Weapon Drop Settings")]
    [SerializeField] private Transform weaponDropPoint;

    [Header("Ability Orb Rewards")]
    [SerializeField] private GameObject[] abilityOrbPrefabs;
    [SerializeField] private Transform abilityDropPoint;
    [Range(0f, 100f)][SerializeField] private float abilityOrbDropChance = 25f;

    [Header("Highlight Settings")]
    [SerializeField] private Renderer model;
    [SerializeField] private Material highlight;



    private Material materialOg;
    private bool isOpen;
    private bool isMoving;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private PlayerWeaponManager weaponManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            materialOg = model.material;
        }

        weaponManager = FindAnyObjectByType<PlayerWeaponManager>();


    }

    public void Interact()
    {
        if (isOpen || isMoving)
        {
            gameManager.instance.interactText.gameObject.SetActive(false);
            RecticleBehaviour.OffHover();
            return;
        }

        gameManager.instance.interactText.gameObject.SetActive(false);
        RecticleBehaviour.OffHover();

        StartCoroutine(OpenChest());
        isOpen = true;
    }

    private IEnumerator OpenChest()
    {
        isMoving = true;
        PlayChestOpenSound();

        bool rewardGiven = false;

        while (Quaternion.Angle(lidTransform.rotation, openRotation) > 0.1f)
        {
            lidTransform.rotation = Quaternion.Slerp(
                lidTransform.rotation,
                openRotation,
                openSpeed * Time.deltaTime
            );

            //Give the reward while the chest is opening instead of waiting until it is fully open.
            if (!rewardGiven && Quaternion.Angle(lidTransform.rotation, closedRotation) > 20f)
            {
                GiveWeaponReward();
                TrySpawnAbilityOrb();
                TrySpawnEnemy();
                rewardGiven = true;
            }

            yield return null;
        }
    }

    private void GiveWeaponReward()
    {
        int rewardCount = weaponRewards.Length;

        if (canGiveMaxAmmo)
        {
            rewardCount += 1;
        }

        if (rewardCount <= 0)
        {
            Debug.LogWarning("Weapon chest has no rewards assigned.");
            return;
        }

        int rewardRoll = Random.Range(0, rewardCount);

        if (canGiveMaxAmmo && rewardRoll == rewardCount - 1)
        {
            GiveMaxAmmo();
            return;
        }

        WeaponReward reward = weaponRewards[rewardRoll];

        if (reward.pickupPrefab == null)
        {
            Debug.LogWarning("Weapon reward is missing a pickup prefab.");
            return;
        }

        Transform dropPoint = weaponDropPoint != null ? weaponDropPoint : transform;

        GameObject spawnedWeapon = Instantiate(
        reward.pickupPrefab,
        dropPoint.position,
        dropPoint.rotation
        );

        WeaponPickUp pickup = spawnedWeapon.GetComponentInChildren<WeaponPickUp>();

        if (pickup != null)
        {
            pickup.RollChestWeaponMods();
        }
        else
        {
            Debug.LogWarning("Spawned weapon pickup does not have a WeaponPickUp script.");
        }

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Weapon Chest: " + reward.weaponName + " dropped");
        }

        Debug.Log("Weapon chest dropped: " + reward.weaponName);
    }

    private void GiveMaxAmmo()
    {
        if (weaponManager.MaxAmmo <= 0)
        {
            Debug.Log("Max ammo reward rolled, but player has no weapon equipped.");
            return;
        }

        weaponManager.Ammo = weaponManager.MaxAmmo;

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Weapon Chest: Max Ammo");
        }

        Debug.Log("Weapon chest gave max ammo!");
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

    private void TrySpawnAbilityOrb()
    {
        if (abilityOrbPrefabs == null || abilityOrbPrefabs.Length == 0)
        {
            return;
        }

        float roll = Random.Range(0f, 100f);

        if (roll > abilityOrbDropChance)
        {
            return;
        }

        Transform dropPoint = abilityDropPoint != null ? abilityDropPoint : weaponDropPoint;

        if (dropPoint == null)
        {
            dropPoint = transform;
        }

        GameObject orbPrefab = abilityOrbPrefabs[Random.Range(0, abilityOrbPrefabs.Length)];

        if (orbPrefab == null)
        {
            return;
        }

        Instantiate(orbPrefab, dropPoint.position, dropPoint.rotation);

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.ShowUpgradeNotification("Ability orb dropped");
        }

        Debug.Log("Weapon chest dropped an ability orb.");
    }

    public void OnHoverEnter()
    {
        if (isOpen)
        {
            gameManager.instance.interactText.gameObject.SetActive(false);
            RecticleBehaviour.OffHover();
            return;
        }

        if (model != null && highlight != null)
        {
            model.material = highlight;
        }

        gameManager.instance.interactText.gameObject.SetActive(true);
        RecticleBehaviour.OnHover(0);
    }

    public void OnHoverExit()
    {
        if (model != null && materialOg != null)
        {
            model.material = materialOg;
        }

        gameManager.instance.interactText.gameObject.SetActive(false);
        RecticleBehaviour.OffHover();
    }

    private void PlayChestOpenSound()
    {
        if (AudioManager.instance != null && chestOpenSound != null)
        {
            AudioManager.instance.PlaySoundAtPosition(chestOpenSound, gameObject);
        }
    }
}