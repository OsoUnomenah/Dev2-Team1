using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] public bool gameDebug;

    [Header ("XP Config")]
    public Slider xpBar;
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

    [Header ("Menu Config")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuSettings;
    [SerializeField] GameObject playerInputHandler;    
    public bool isPaused;

    [Header("Sprint Config")]
    public bool SprintTriggered;
    public bool canSprint;
    public bool isSprinting;

    public int sprintCost;

    [Header("Player Config")]
    public GameObject player;
    public GameObject playerController;
    public GameObject playerStatHandler;

    float timeScaleOrig;

    int gameGoalCount;

    public float recoil;
    public bool canShoot;

    public int enemyDamageOut;

    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;
        player = GameObject.FindGameObjectWithTag("Player");
        playerInputHandler = GameObject.FindGameObjectWithTag("PlayerInputHandler");
        playerStatHandler = GameObject.FindGameObjectWithTag("PlayerStatHandler");
        
    }

    // Update is called once per frame
    void Update()
    {
        //XP requirement is based on the player's current level
        xpToNextLevel = 10 + (level * 10);



        //Update XP text only if the text reference exists
        //XP is no longer gained over time here. XP should come from addXp()
        //Which is called when enemies die or when another reward gives XP
        if (xpText != null)
        {
            xpText.text = "LVL: " + level + " XP: " + currentXP + " / " + xpToNextLevel;
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

    public void PauseGame()
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }

    public void addXp(int amount)
    {
        
        currentXP += amount;

        //Show how much XP was just earned
        if (xpBoostText != null)
        {
            xpBoostText.SetText(" + " + amount + " XP");
        }

        //Handles leveling up when enough XP is gained
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            levelUp();

            xpToNextLevel = 10 + (level * 10);
        }

    }
    public void levelUp()
    {
        ++level;

        //level up logic here

        if (gameDebug)
        Debug.Log("Gained a Level!");
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

    public void updateGameGoal(int amount)
    {
        //Currently a kill all enemies goal, will be expanded on in the future
        gameGoalCount += amount;
        if (gameGoalCount <= 0)
        {
            gameManager.instance.statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);

        }
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
        //else if (menuActive == menuPause)
        //{
        //    stateUnpause();
        //}
    }

    public void settings()
    {
        menuPause.SetActive(false);
        menuActive = null;
        menuActive = menuSettings;
        menuActive.SetActive(true);
    }
}
