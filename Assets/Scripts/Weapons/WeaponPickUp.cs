using UnityEngine;

public class WeaponPickUp : MonoBehaviour, IInteract
{
    string objectName;
    
    [SerializeField] Renderer model;
    Material materialOrig;
    [SerializeField] Material highLight;
   
    [SerializeField] bool weaponType; //true for melee false for projectile
    [SerializeField] int damage;
    [SerializeField] float range;
    [SerializeField] float rate;
    [SerializeField] float recoil;
    [SerializeField] float timer;

    private PlayerWeaponManager weaponManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materialOrig = model.material;
        weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        objectName = gameObject.name;
    }    

    public void Interact()
    { 
        Debug.Log($"Picked up {objectName}");        

        weaponManager.Equip(weaponType, damage, range, rate, recoil, timer);

        Destroy(gameObject);
    }

    public void OnHoverEnter()
    {
        model.material = highLight;
    }

    public void OnHoverExit()
    {       
        model.material = materialOrig;
    }
}
