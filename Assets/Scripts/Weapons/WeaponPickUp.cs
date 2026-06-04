using UnityEngine;

public class WeaponPickUp : MonoBehaviour, IInteract
{
    [SerializeField] Renderer model;

    Material materialOrig;
   [SerializeField] Material highLight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materialOrig = model.material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }    

    public void Interact()
    {

        string objectName = gameObject.name;

        Debug.Log($"Picked up {objectName}");

        Destroy(gameObject);
    }

    public void OnHoverEnter()
    {
        model.material = highLight;
    }

    void OnHoverExit()
    {
        model.material = materialOrig;
    }
}
