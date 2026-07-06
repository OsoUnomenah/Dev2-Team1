using UnityEngine;
using TMPro;
using System.Collections;
public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI instance;

    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private TMP_Text notifyText;
    [SerializeField] private StatHandler playerStats;

    private Coroutine notifyRoutine;

    private void Awake()
    {
        
        instance = this;

    }

    private void Start()
    {
        
        if (playerStats == null)
        {
            GameObject statObj = GameObject.FindGameObjectWithTag("PlayerStatHandler");

            if(statObj != null)
            {
                playerStats = statObj.GetComponent<StatHandler>();
            }
        }

        if (notifyText != null)
        {
            notifyText.gameObject.SetActive(false);
        }

        UpdateUpgradeText();

    }

    public void UpdateUpgradeText()
    {
        if (upgradeText == null || playerStats == null)
            return;

        upgradeText.text = 
            "Current Upgrades\n" + 
            "DMG +" + playerStats.modDamage + "\n" + 
            "DEF +" + playerStats.modDefense + "\n" +
            "HP +" + playerStats.modHealth + "\n" +
            "STM +" + playerStats.modStamina + "\n" +
            "SPD +" + playerStats.modSpeed + "\n" +
            "JMP +" + playerStats.modJumps;

    }

    public void ShowUpgradeNotification(string message)
    {
        if (notifyText == null)
        {
            return;
        }

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
