using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour, IModifier
{
    public enum ModTypes
    {
        Damage,
        Defence,
        Speed,
        MaxHP,
        MaxJumps,
        MaxStamina
    }

    [Header("Don't touch unless debugging")]
    [SerializeField] private ModTypes modType;
    [SerializeField] private int mod;

    //***IMPORTANT***
    //
    //the Mod type 0 (zero) is ALWAYS a weapon's mod. if you change that
    //finding and deleting weapons WILL break!!
    [SerializeField] List<int> Modifiers = new List<int>(); 
                                            //each mod is 2 slots of the list
                                          // slot 1: 1 == Damage mod type
                                          // slot 2: 12 = the modified amount

    [SerializeField] private int ammount;

    private GameObject obj;

    private int enumCount = System.Enum.GetValues(typeof(ModTypes)).Length;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }
    public void OnLevelUp()
    {
        obj = GameObject.FindGameObjectWithTag("Player");
        createMod();
        addMod();
    }
    public void OnEnemySpawn()
    {
        obj = GameObject.FindGameObjectWithTag("Enemy");
        createMod();
        addMod();
    }
    public void OnWeaponSpawn()
    {
        obj = GameObject.FindGameObjectWithTag("Weapon");
        createMod();
        
        //weapon mod type
        //so that the delete function will work
        mod = 0;

        addMod();
    } 

    public void addMod()
    {
        Modifiers.Add(mod);
        Modifiers.Add(ammount);
    }

    public void removeMod()
    {
        //this is only for finding and deleting the current weapon mods when switching weapons
        for(int i = 0; i < Modifiers.Count; i++)
        {
            if (Modifiers[i] == 0)
            {
                Modifiers.RemoveAt(i);
                Modifiers.RemoveAt(i);
            }
        }
    }

    public void replaceMod()
    {
        throw new System.NotImplementedException();
    }

    public void createMod()
    {
        mod = Random.Range(1, enumCount);
        
        switch(mod)
        { 
            //these can and should be fine tuned eventually

            case 1: //Damage +/-
                ammount = Random.Range(1, 40);
                break;
            case 2: //Defence +/-
                ammount = Random.Range(1, 20);
                break;
            case 3: //Speed +/-
                ammount = Random.Range(1, 12);
                break;
            case 4: //MaxHP +/-
                ammount = Random.Range(1, 40);
                break;
            case 5: //Jumps +/-
                ammount = Random.Range(1, 3);
                break;
            case 6: //Stamina +/-
                ammount = Random.Range(1, 100);
                break;
        }
    }
}
