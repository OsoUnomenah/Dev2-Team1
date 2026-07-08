//using TMPro;
//using UnityEngine;

//public class WeaponInventoryUI : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private PlayerWeaponManager weaponManager;

//    [Header("Weapon Slot Text")]
//    [SerializeField] private TMP_Text slot1Text;
//    [SerializeField] private TMP_Text slot2Text;
//    [SerializeField] private TMP_Text slot3Text;
//    [SerializeField] private TMP_Text slot4Text;

//    [SerializeField] private TMP_Text currentWeaponText;

//    private void Start()
//    {
//        if (weaponManager == null)
//        {
//            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
//        }

//        UpdateWeaponInventoryUI();
//    }

//    private void Update()
//    {
//        UpdateWeaponInventoryUI();
//    }

//    private void UpdateWeaponInventoryUI()
//    {
//        if (weaponManager == null)
//        {
//            return;
//        }

//        if (slot1Text != null)
//        {
//            slot1Text.text = GetSlotText(0);
//        }

//        if (slot2Text != null)
//        {
//            slot2Text.text = GetSlotText(1);
//        }

//        if (slot3Text != null)
//        {
//            slot3Text.text = GetSlotText(2);
//        }

//        if (slot4Text != null)
//        {
//            slot4Text.text = GetSlotText(3);
//        }

//        if (currentWeaponText != null)
//        {
//            currentWeaponText.text = "Current: " + weaponManager.GetWeaponNameAtSlot(weaponManager.CurrentWeaponIndex);
//        }
//    }

//    private string GetSlotText(int index)
//    {
//        string weaponName = weaponManager.GetWeaponNameAtSlot(index);

//        if (index == weaponManager.CurrentWeaponIndex)
//        {
//            return "> " + (index + 1) + ": " + weaponName;
//        }

//        return (index + 1) + ": " + weaponName;
//    }
//}

//kw no longer needed