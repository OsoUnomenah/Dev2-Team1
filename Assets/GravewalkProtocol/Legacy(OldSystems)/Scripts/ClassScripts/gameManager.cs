using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] public bool gameDebug;
    public TMP_Text objectiveText;

    public GameEvent onPlayerHealthChange;

    [Header("XP Config")]
    public Slider xpBar;
    public Slider reloadBar;
    public TMP_Text xpText;
    public TMP_Text xpBoostText;
    private TMP_Text xpBOrig;


    [Header("Level Config")]
    [Range(1, 100)][SerializeField] public float level;
    [Range(1, 1000)][SerializeField] public float maxLevel;
    [Range(1, 1000)][SerializeField] public float xp;
    public float currentXP;
    public float xpSource;
    [SerializeField] public float xpToNextLevel;
    [Range(0, 1)][SerializeField] public float xpGain;
    public float currentLevel;

    [Header("Menu Config")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuSettings;

    [SerializeField] public GameObject playerDamageFlash;
    [SerializeField] public GameObject playerHealFlash;
    [SerializeField] public GameObject checkpointUI;

    [SerializeField] public GameObject Reload;
    [SerializeField] public float reloadTime;
    [SerializeField] public float reloadMax;
    [SerializeField] public TextMeshProUGUI interactText;
    public bool isPaused;
    public bool isLevelingUp;

    public TMP_Text waveText;

    [Header("Sprint Config")]
    public bool dashTriggered;
    public bool canDash;
    public bool isDashing;
    public int dashCost;

    [Header("Player References")]
    [SerializeField] public GameObject player;
    [SerializeField] public Camera playerCamera;
    [SerializeField] public CharacterController characterController;
    [SerializeField] public PlayerInputHandler playerInputHandler;
    [SerializeField] public StatHandler playerStatHandler;
    [SerializeField] public PlayerWeaponManager playerWeaponManager;
    [SerializeField] public Transform playerTransform;
    [SerializeField] public Players playerInteract;

    public GameObject playerSpawnPos;

    [Header("Charged Shot UI")]
    [SerializeField] public GameObject sniperChargePanel;
    [SerializeField] public Slider sniperChargeSlider;
    [SerializeField] public TMP_Text sniperChargeText;

    [SerializeField] public AbilityUI abilityUI;
    public bool allowedAbility1 = true;
    public bool allowedAbility2 = true;
    public bool allowedAbility3 = true;
    public bool allowedAbility4 = true;
    [SerializeField] public int firePos = -1;
    [SerializeField] public int freezePos = -1;
    [SerializeField] public int magnetPos = -1;
    [SerializeField] public int toxicPos = -1;
    [SerializeField] public int crystalPos = -1;
    [SerializeField] public int lightningPos = -1;

    [Header("Currency")]
    [SerializeField] private int currentCurrency;
    public int CurrentCurrency => currentCurrency;
    public TMP_Text currencyText;

    float timeScaleOrig;
    int gameGoalCount;

    public float recoil;
    public bool canShoot;
    public bool isShooting;
    public bool isMeleeing;
    public bool canMelee;
    public bool isReloading;
    public bool isAiming;
    public int enemyDamageOut;
    public int playerDamageOut;

    [Header("Roguelite Run Config")]
    public int runZone = 1;

    [Header("Win Config")]
    public Button nextLevelButton;


    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitGM();
        CacheTimeScale();
        GetPlayerReferences();
        UpdateXPUI();
        UpdateCurrencyUI();
        abilityUI = FindAnyObjectByType<AbilityUI>();
        playerSpawnPos = GameObject.FindGameObjectWithTag("PlayerSpawnPos");
    }

    private void Start()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(NextLevel);
        }
        //set player initial spawn point
        //playerTransform.position = playerSpawnPoint.transform.position;
        menuWin.SetActive(false);
    }

    private void InitGM()
    {
        instance = this;
    }

    private void CacheTimeScale()
    {
        timeScaleOrig = Time.timeScale;
    }

    private void GetPlayerReferences()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        characterController = player.GetComponentInChildren<CharacterController>();
        playerInputHandler = player.GetComponentInChildren<PlayerInputHandler>();
        playerStatHandler = player.GetComponentInChildren<StatHandler>();
        playerWeaponManager = player.GetComponentInChildren<PlayerWeaponManager>();
        playerCamera = player.GetComponentInChildren<Camera>();
        playerTransform = player.GetComponent<Transform>();
        playerInteract = player.GetComponent<Players>();


        if (gameDebug)
        {
            Debug.Log("Player: " + player);
            Debug.Log("CharacterController: " + characterController);
            Debug.Log("PlayerInputHandler: " + playerInputHandler);
            Debug.Log("StatHandler: " + playerStatHandler);
            Debug.Log("PlayerWeaponManager: " + playerWeaponManager);
            Debug.Log("PlayerCamera: " + playerCamera);
            Debug.Log("PlayerPosition: " + playerTransform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //change xpGain value in inspector to adjust rate.
        //Need to be in update for level function until refactored to be event based instead of update based.
        PassiveXP();
    }
    public void slotFiller()
    {
        //fills the slots list that remembers where each bullet type is in
        if (instance.playerWeaponManager.abilities.Count == 4)
        {
            Debug.LogError("Does not Run");
            return;
        }
        {
            AbilityStats slot = instance.playerWeaponManager.abilities[instance.playerWeaponManager.abilitySlot];

            if (slot == null) return;

            if (slot.abilityType == AbilityStats.ability.fire)
            {
                firePos = instance.playerWeaponManager.abilitySlot;
                //Debug.LogError("FIRE");
                return;
            }
            if (slot.abilityType == AbilityStats.ability.freeze)
            {
                freezePos = instance.playerWeaponManager.abilitySlot;
                //Debug.LogError("FREEZE");
                return;
            }
            if (slot.abilityType == AbilityStats.ability.magent)
            {
                magnetPos = instance.playerWeaponManager.abilitySlot;
                //Debug.LogError("MAGNET");
                return;
            }
            if (slot.abilityType == AbilityStats.ability.toxic)
            {
                toxicPos = instance.playerWeaponManager.abilitySlot;
                //Debug.LogError("TOXIC");
                return;
            }
            if (slot.abilityType == AbilityStats.ability.crystal)
            {
                crystalPos = instance.playerWeaponManager.abilitySlot;
                //Debug.LogError("CRYSTAL");
                return;
            }
            if (slot.abilityType == AbilityStats.ability.lightning)
            {
                lightningPos = instance.playerWeaponManager.abilitySlot;
                //Debug.LogError("LIGHTNING");
                return;
            }
            Debug.LogError("Found None");
        }
        Debug.LogError("FAILURE");
    }
    public void greyedOut(float cd, int slot)
    {
        StartCoroutine(greyHandler(cd, slot));
    }
    IEnumerator greyHandler(float cd, int slot)
    {

        switch (slot)
        {
            case 0:
                abilityUI.grey1.SetActive(true);
                break;
            case 1:
                abilityUI.grey2.SetActive(true);
                break;
            case 2:
                abilityUI.grey3.SetActive(true);
                break;
            case 3:
                abilityUI.grey4.SetActive(true);
                break;
        }

        yield return new WaitForSeconds(cd);

        switch (slot)
        {
            case 0:
                abilityUI.grey1.SetActive(false);
                break;
            case 1:
                abilityUI.grey2.SetActive(false);
                break;
            case 2:
                abilityUI.grey3.SetActive(false);
                break;
            case 3:
                abilityUI.grey4.SetActive(false);
                break;
        }

    }

    private void UpdateXPUI()
    {
        //XP requirement is based on the player's current level
        xpToNextLevel = 10 + (level * 10);

        //Update XP text only if the text reference exists
        //XP is no longer gained over time here. XP should come from addXp()
        //Which is called when enemies die or when another reward gives XP
        if (xpText != null)
        {
            xpText.text = "LVL: " + level + " XP: " + (int)currentXP + " / " + xpToNextLevel;
        }

        //Clear the XP boost text by default
        if (xpBoostText != null)
        {
            xpBoostText.text = "";
        }

        //Update XP bar based on current XP progress toward the next level
        if (xpBar != null)
        {
            xpBar.value = currentXP / xpToNextLevel;
        }
    }

    private void PassiveXP()
    {
        if (!LevelUpUI.Instance.isChoosing && !gameManager.instance.isPaused)
        {
            currentXP += xpGain;
            UpdateXPUI();
            //Handles leveling up when enough XP is gained
            while (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                levelUp();

                xpToNextLevel = 10 + (level * 10);
            }
        }
    }

    public void addXp(int amount)
    {

        currentXP += amount;
        UpdateXPUI();
    }
    public void levelUp()
    {
        ++level;

        //level up logic here

        if (!isPaused && LevelUpUI.Instance != null) //only show lvl up choices in active gameplay, prevents lvl up screen from popping up over win/lose/pause menu
        {
            LevelUpUI.Instance.ShowLevelUpOptions();
        }

        if (gameDebug)
        {
            Debug.Log("Gained a Level!");
        }

    }

    private void UpdateObjectiveTextUI()
    {
        //Objective text update
        objectiveText.text = "Objective:\nKill the BOSS: " + gameGoalCount;
    }

    public void updateGameGoal(int amount)
    {
        //Currently a kill all enemies goal, will be expanded on in the future
        gameGoalCount += amount;
        UpdateObjectiveTextUI();

        if (gameGoalCount <= 0)
        {
            Debug.Log("Boss Degeated - Open Portal");
        }
    }

    public void PauseGame()
    {
        if (menuActive == null)
        {
            if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.HideForPause();
            }

            statePause();
            menuActive = menuPause;
            menuActive.SetActive(true);
        }
        else if (menuActive == menuPause)
        {
            stateUnpause();

            if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.ShowAfterPause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void WinGame()
    {
        statePause();

        menuActive = menuWin;
        menuActive.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void back()
    {
        if (menuActive == menuSettings)
        {
            menuSettings.SetActive(false);
            menuActive = null;
            menuActive = menuPause;
            menuActive.SetActive(true);
        }

    }

    public void settings()
    {
        menuPause.SetActive(false);
        menuActive = null;
        menuActive = menuSettings;
        menuActive.SetActive(true);
    }

    public void NextZone()
    {
        runZone++;
        Debug.Log("Entered Zone: " + runZone);
    }

    public void updatePlayerUI()
    {
        playerStatHandler.currentHealth = playerStatHandler.maxHealth;
    }

    public void respawnPlayer()
    {
        characterController.transform.position = gameManager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();
        updatePlayerUI();
        onPlayerHealthChange.Raise(this, this);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    public void addCurrency(int amount)
    {
        currentCurrency += amount;
        UpdateCurrencyUI();
    }

    private void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = "Currency: " + currentCurrency;
        }
    }

}