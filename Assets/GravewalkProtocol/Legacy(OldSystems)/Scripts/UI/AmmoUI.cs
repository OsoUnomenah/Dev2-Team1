using TMPro;
using UnityEngine;
public class AmmoUI : MonoBehaviour
{

    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private PlayerWeaponManager weaponManager;


    private void Start()
    {
        if (weaponManager == null)
        {
            weaponManager = gameManager.instance.playerWeaponManager;
        }

        UpdateAmmoText();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAmmoText();
    }

    private void UpdateAmmoText()
    {
        if (ammoText == null || weaponManager == null)
        {
            return;
        }

        if (weaponManager.Type || weaponManager.MaxAmmo <= 0)
        {
            ammoText.text = "-- / --";
            return;
        }

        ammoText.text =
            weaponManager.Ammo +
            " / " +
            weaponManager.ReserveAmmo;
    }
}
