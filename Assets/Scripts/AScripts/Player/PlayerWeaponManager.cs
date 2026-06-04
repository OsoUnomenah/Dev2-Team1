using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    //Weapon Settings
    public bool Type;
    public int Damage;
    public float Range;
    public float Rate;
    public float Recoil;
    public float Timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Equip(bool type, int damage, float range, float rate, float recoil, float timer)
    {
        Type = type;
        Damage = damage;
        Range = range;
        Rate = rate;
        Recoil = recoil;
        Timer = timer;
    }
}
