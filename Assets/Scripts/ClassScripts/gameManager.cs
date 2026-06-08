using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] public bool gameDebug;

    public Slider xpBar;
    public TMP_Text xpText;
    public TMP_Text xpBoostText;

    [Range(1, 100)][SerializeField] public float level;
     public float currentLevel;
    [Range(1, 1000)][SerializeField] public float maxLevel;
    [Range(1, 1000)][SerializeField] public float xp;
    public float currentXP;
    [SerializeField] public float xpToNextLevel;
    [Range(0,1)][SerializeField] public float xpGain;
    public TMP_Text xpBOrig;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuSettings;
    [SerializeField] GameObject playerInputHandler;

    
    public bool isPaused;
    public GameObject player;
    public GameObject playerController;
    public GameObject playerStatHandler;

    float timeScaleOrig;

    int gameGoalCount;

    public float recoil;
    public bool canShoot;

    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;
        player = GameObject.FindGameObjectWithTag("Player");
        playerInputHandler = GameObject.FindGameObjectWithTag("PlayerInputHandler");
    }

    // Update is called once per frame
    void Update()
    {
        xpText.text = " LVL: " + level;
        xpBar.value = (float)currentXP / (float)xpToNextLevel;
        xpBoostText.text = (" + " + xpGain + " XP");
        xpBOrig = xpBoostText;
        xpToNextLevel = 10 + (currentLevel * 10);



        if (!isPaused)
        {
            currentXP += xpGain;
        }

        if (currentXP >= xpToNextLevel)
        {
            levelUp();
            currentXP = 1;

        }


        if (Input.GetButtonDown("Cancel"))
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
    }

    public void addXp(int amount)
    {
        currentXP += amount;

        xpBoostText.SetText(" + " + amount);
        xpBoostText.CrossFadeAlpha(1, 1, false);
        xpBoostText = xpBOrig;


    }
    public void levelUp()
    {
        ++level;
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
