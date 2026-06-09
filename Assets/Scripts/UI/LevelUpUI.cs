using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
public class LevelUpUI : MonoBehaviour
{
   
   public static LevelUpUI Instance;

    private enum upgradeType
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

    private class UpgradeOption
    {
        public upgradeType upgradeType;
        public Rarity rarity;
        public float amount;
        public string displayText;
    }

    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private Button optionButton1;
    [SerializeField] private Button optionButton2;
    [SerializeField] private Button optionButton3;

    [SerializeField] private TMP_Text optionText1;
    [SerializeField] private TMP_Text optionText2;
    [SerializeField] private TMP_Text optionText3;

    [Header("Player References")]
    [SerializeField] private StatHandler playerStats;

    [Header("Level Up Settings")]
    [SerializeField] private float slowMotionScale = 0.25f; //so time slows down on lvl up and doesnt completely stop
    [SerializeField] private float choiceTime = 10f; //amount of time to select upgrade before game resumes

    private UpgradeOption[] currentOptions = new UpgradeOption[3];
    private Coroutine countdownRoutine;
    private float originalTimeScale;
    private bool isChoosing;
    private bool wasHiddenbyPause;
    private bool timerPaused;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        if(playerStats == null)
        {
            GameObject statObj = GameObject.FindGameObjectWithTag("PlayerStatHandler");

            if (statObj != null)
            {
                playerStats = statObj.GetComponent<StatHandler>();
            }
        }

        optionButton1.onClick.AddListener(() => PickOption(0));
        optionButton2.onClick.AddListener(() => PickOption(1));
        optionButton3.onClick.AddListener(() => PickOption(2));
    }

    public void ShowLevelUpOptions()
    {
        if (isChoosing)
            return;

        if (levelUpPanel == null || playerStats == null)
            return;

        isChoosing = true;

        originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionScale;
        gameManager.instance.isLevelingUp = true;

        levelUpPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        RollOptions();
        UpdateButtons();

        if(countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }

        countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    private void RollOptions()
    {
        for (int i = 0; i < currentOptions.Length; i++)
        {
            upgradeType upgrade = (upgradeType)Random.Range(0, System.Enum.GetValues(typeof(upgradeType)).Length);
            Rarity rarity = RollRarity();
            float amount = GetAmount(upgrade, rarity);

            currentOptions[i] = new UpgradeOption();
            currentOptions[i].upgradeType = upgrade;
            currentOptions[i].rarity = rarity;
            currentOptions[i].amount = amount;
            currentOptions[i].displayText = rarity + "\n" + upgrade + "\n+" + amount;
        }
    }

    private void UpdateButtons()
    {
        optionText1.text = currentOptions[0].displayText;
        optionText2.text = currentOptions[1].displayText;
        optionText3.text = currentOptions[2].displayText;

        SetButtonColor(optionButton1, currentOptions[0].rarity);
        SetButtonColor(optionButton2, currentOptions[1].rarity);
        SetButtonColor(optionButton3, currentOptions[2].rarity);
    }

    private IEnumerator CountdownRoutine()
    {
        float timer = choiceTime;

        while (timer > 0)
        {
            if (!timerPaused)
            {
                timer -= Time.unscaledDeltaTime;
            }

            if (countdownText != null)
            {
                countdownText.text = "Choose an upgrade: " + Mathf.CeilToInt(timer);
            }

            yield return null;
        }

        CloseLevelUpPanel();
    }

    private void PickOption(int optionIndex)
    {
        if (isChoosing)
            if (!isChoosing)
            {
                return;
            }

        ApplyUpgrade(currentOptions[optionIndex]);

        if (UpgradeUI.instance != null)
        {
            UpgradeUI.instance.UpdateUpgradeText();
            UpgradeUI.instance.ShowUpgradeNotification(currentOptions[optionIndex].rarity + " " + currentOptions[optionIndex].upgradeType + " +" + currentOptions[optionIndex].amount);
        }

        CloseLevelUpPanel();
    }

    private void CloseLevelUpPanel()
    {
        isChoosing = false;
        wasHiddenbyPause = false;
        timerPaused = false;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        if (gameManager.instance != null)
        {
            gameManager.instance.isLevelingUp = false;

            if (gameManager.instance.isPaused)
            {
                Time.timeScale = 0;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                return;
            }
        }

        Time.timeScale = originalTimeScale;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private Rarity RollRarity()
    {
        int roll = Random.Range(1, 101);

        if(roll <= 60)
        {
            return Rarity.Common;
        }
        else if(roll <=85)
        {
            return Rarity.Rare;
        }
        else if(roll<=97)
        {
            return Rarity.Epic;
        }
        else
        {
            return Rarity.Legendary;
        }
    }

    private float GetAmount(upgradeType upgrade, Rarity rarity)
    {
        switch(upgrade)
        {
            case upgradeType.Damage:
                return GetRarityAmount(rarity, 2, 5, 10, 20);
            case upgradeType.Defense:
                return GetRarityAmount(rarity, 1, 3, 6, 10);
            case upgradeType.MaxHealth:
                return GetRarityAmount(rarity, 10, 20, 35, 50);
            case upgradeType.MaxStamina:
                return GetRarityAmount(rarity, 10, 20, 35, 50);
            case upgradeType.Speed:
                return GetRarityAmount(rarity, 1, 2, 3, 5);
            case upgradeType.Jumps:
                return GetRarityAmount(rarity, 1, 1, 2, 3);
        }

        return 0;
    }

    private float GetRarityAmount(Rarity rarity, float common, float rare, float epic, float legendary)
    {
        switch(rarity)
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

    private void ApplyUpgrade(UpgradeOption option)
    {
        switch(option.upgradeType)
        {
            case upgradeType.Damage:
                playerStats.modDamage += option.amount;
                break;

            case upgradeType.Defense:
                playerStats.modDefense += option.amount;
                break;

            case upgradeType.MaxHealth:
                playerStats.modHealth += option.amount;
                playerStats.maxHealth += option.amount;
                playerStats.currentHealth += option.amount;
                break;

            case upgradeType.MaxStamina:
                playerStats.modStamina += option.amount;
                playerStats.maxStamina += option.amount;
                playerStats.currentStamina += option.amount;
                break;

            case upgradeType.Speed:
                playerStats.modSpeed += option.amount;
                break;

            case upgradeType.Jumps:
                playerStats.modJumps += option.amount;
                break;
        }
    }

    private void SetButtonColor(Button button, Rarity rarity)
    {
        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage == null)
            return;

        switch(rarity)
        {
            case Rarity.Common:
                buttonImage.color = Color.gray;
                break;

            case Rarity.Rare:
                buttonImage.color = Color.blue;
                break;

            case Rarity.Epic:
                buttonImage.color = new Color(0.5f, 0f, 1f);
                break;

            case Rarity.Legendary:
                buttonImage.color = new Color(1f, 0.65f, 0f); ;
                break;

        }
    }

    public void HideForPause()
    {
        if (!isChoosing)
        {
            return;
        }

        wasHiddenbyPause = true;
        timerPaused = true;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        //Keep level-up state active, but hidden while pause menu is open.
        if (gameManager.instance != null)
        {
            gameManager.instance.isLevelingUp = true;
        }
    }

    public void ShowAfterPause()
    {
        if (!wasHiddenbyPause)
        {
            return;
        }

        wasHiddenbyPause = false;
        timerPaused = false;
        isChoosing = true;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        if (gameManager.instance != null)
        {
            gameManager.instance.isLevelingUp = true;
        }

        Time.timeScale = slowMotionScale;

        //Make sure the cursor comes back after resuming from pause
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

}
