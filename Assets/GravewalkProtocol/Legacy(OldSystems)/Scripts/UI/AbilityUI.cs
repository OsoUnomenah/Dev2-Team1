using System.Collections.Generic;
using UnityEngine;

public class AbilityUI : MonoBehaviour
{
    [Header("Ability Slots")]
    [SerializeField] private GameObject slot1;
    [SerializeField] private GameObject slot2;
    [SerializeField] private GameObject slot3;
    [SerializeField] private GameObject slot4;
    [SerializeField] private GameObject slot5;
    [SerializeField] private GameObject slot6;

    [Header("Ability Icons")]
    [SerializeField] private GameObject Fire;
    [SerializeField] private GameObject Freeze;
    [SerializeField] private GameObject Magnet;
    [SerializeField] private GameObject Toxic;
    [SerializeField] private GameObject Crystal;
    [SerializeField] private GameObject Lightning;

    [Header("Slot Number Objects")]
    [SerializeField] private GameObject num1;
    [SerializeField] private GameObject num2;
    [SerializeField] private GameObject num3;
    [SerializeField] private GameObject num4;
    [SerializeField] private GameObject num5;
    [SerializeField] private GameObject num6;

    [Header("Legacy Selection Overlays")]
    [SerializeField] public GameObject grey1;
    [SerializeField] public GameObject grey2;
    [SerializeField] public GameObject grey3;
    [SerializeField] public GameObject grey4;
    [SerializeField] public GameObject grey5;
    [SerializeField] public GameObject grey6;

    [Header("Collected Abilities")]
    [SerializeField] public List<GameObject> abilitySlots = new List<GameObject>();

    private GameObject[] slots;
    private GameObject[] numbers;

    private void Awake()
    {
        slots = new GameObject[]
        {
            slot1,
            slot2,
            slot3,
            slot4,
            slot5,
            slot6
        };

        numbers = new GameObject[]
        {
            num1,
            num2,
            num3,
            num4,
            num5,
            num6
        };

        HideEmptySlots();

        SetCooldownOverlays(false);
    }

    public void abilityAssign(AbilityStats.ability type)
    {
        GameObject abilityPrefab = GetAbilityPrefab(type);

        if (abilityPrefab == null)
        {
            return;
        }

        int slotIndex = abilitySlots.Count;

        if (slotIndex >= slots.Length)
        {
            return;
        }

        GameObject slot = slots[slotIndex];

        if (slot == null)
        {
            return;
        }

        GameObject abilityIcon = Instantiate(abilityPrefab, slot.transform);

        RectTransform iconRect = abilityIcon.GetComponent<RectTransform>();

        if (iconRect != null)
        {
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(4f, 4f);
            iconRect.offsetMax = new Vector2(-4f, -4f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.localScale = Vector3.one;
        }

        abilityIcon.SetActive(true);
        slot.SetActive(true);


        gameManager.instance.playerWeaponManager.abilitySlot = slotIndex;
        abilitySlots.Add(abilityIcon);
    }

    private GameObject GetAbilityPrefab(AbilityStats.ability type)
    {
        switch (type)
        {
            case AbilityStats.ability.fire:
                return Fire;

            case AbilityStats.ability.freeze:
                return Freeze;

            case AbilityStats.ability.magent:
                return Magnet;

            case AbilityStats.ability.toxic:
                return Toxic;

            case AbilityStats.ability.crystal:
                return Crystal;

            case AbilityStats.ability.lightning:
                return Lightning;

            default:
                return null;
        }
    }

    private void HideEmptySlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (numbers[i] != null)
            {
                numbers[i].SetActive(false);
            }

            if (slots[i] != null)
            {
                slots[i].SetActive(false);
            }
        }
    }

    private void SetCooldownOverlays(bool isActive)
    {
        if (grey1 != null) grey1.SetActive(isActive);
        if (grey2 != null) grey2.SetActive(isActive);
        if (grey3 != null) grey3.SetActive(isActive);
        if (grey4 != null) grey4.SetActive(isActive);
        if (grey5 != null) grey5.SetActive(isActive);
        if (grey6 != null) grey6.SetActive(isActive);
    }
}