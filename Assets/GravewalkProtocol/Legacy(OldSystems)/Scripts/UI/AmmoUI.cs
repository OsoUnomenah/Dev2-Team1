using UnityEngine;
using TMPro;
public class AmmoUI : MonoBehaviour
{

    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private PlayerWeaponManager weaponManager;


    private void Start()
    {
        if(weaponManager == null)
        {
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
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
            return;

        if(weaponManager.MaxAmmo <= 0)
        {
            ammoText.text = "Ammo: -- / --";
            return;
        }

        ammoText.text = "Ammo: " + weaponManager.Ammo + " / " + weaponManager.MaxAmmo;
    }
}
