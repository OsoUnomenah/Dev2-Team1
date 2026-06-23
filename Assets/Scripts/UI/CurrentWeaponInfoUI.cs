using TMPro;
using UnityEngine;

public class CurrentWeaponInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private PlayerWeaponManager weaponManager;

    private void Start()
    {
        if (weaponManager == null)
        {
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
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
            panel.SetActive(true);
        }
        else
        {
            panel.SetActive(false);
        }
    }
}
