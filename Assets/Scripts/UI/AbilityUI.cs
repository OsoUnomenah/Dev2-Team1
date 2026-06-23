using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] GameObject slot1;
    [SerializeField] GameObject slot2;
    [SerializeField] GameObject slot3;
    [SerializeField] GameObject slot4;

    [SerializeField] GameObject Fire;
    [SerializeField] GameObject Freeze;
    [SerializeField] GameObject Bounce;
    [SerializeField] GameObject Zoom;

    [SerializeField] GameObject num1;
    [SerializeField] GameObject num2;
    [SerializeField] GameObject num3;
    [SerializeField] GameObject num4;

    [SerializeField] public GameObject grey1;
    [SerializeField] public GameObject grey2;
    [SerializeField] public GameObject grey3;
    [SerializeField] public GameObject grey4;

    [SerializeField] public List<GameObject> abilitySlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void abilityAssign(int type)
    {
        GameObject ability = null;

        switch (type)
        {
            case 1:
                ability = Fire;
                break;
            case 2: 
                ability = Freeze; 
                break;
            case 3:
                ability = Bounce;
                break;
            case 4:
                ability = Zoom;
                break;
        }

        if (ability == null)
            return;

        if (slot1.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot1.transform);
            obj.SetActive(true);
            num1.SetActive(true);
            gameManager.instance.playerWeaponManager.abilitySlot = 0;
            abilitySlots.Add(obj);
            grey1.SetActive(false);
        }
        else if (slot2.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot2.transform);
            obj.SetActive(true);
            num2.SetActive(true);
            gameManager.instance.playerWeaponManager.abilitySlot = 1;
            abilitySlots.Add(obj);
            grey2.SetActive(false);
        }
        else if (slot3.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot3.transform);
            obj.SetActive(true);
            num3.SetActive(true);
            gameManager.instance.playerWeaponManager.abilitySlot = 2;
            abilitySlots.Add(obj);
            grey3.SetActive(false);
        }
        else if (slot4.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot4.transform);
            obj.SetActive(true);
            num4.SetActive(true);
            gameManager.instance.playerWeaponManager.abilitySlot = 3;
            abilitySlots.Add(obj);
            grey4.SetActive(false);
        }
    }

   
}
