using TMPro;
using UnityEngine;

public class CurrentWeaponInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject upgradeModPanel;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private PlayerWeaponManager weaponManager;

    public TMP_Text infoText2 => infoText;

    private bool tabHeld;

    private void Start()
    {
        if (weaponManager == null)
        {
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
        }

        SetPanelsActive(false);
    }

    private void Update()
    {
        if (panel == null || infoText == null || weaponManager == null)
        {
            return;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            infoText.text = weaponManager.GetCurrentWeaponInfoText();
            SetPanelsActive(true);
            tabHeld = true;
        }
        else if (tabHeld)
        {
            SetPanelsActive(false);
            tabHeld = false;
        }
    }

    private void SetPanelsActive(bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }

        if (upgradeModPanel != null)
        {
            upgradeModPanel.SetActive(isActive);
        }
    }
}