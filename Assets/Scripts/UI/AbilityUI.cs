using UnityEngine;

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
        }
        else if (slot2.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot2.transform);
            obj.SetActive(true);
            num2.SetActive(true);
        }
        else if (slot3.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot3.transform);
            obj.SetActive(true);
            num3.SetActive(true);
        }
        else if (slot4.transform.childCount == 0)
        {
            GameObject obj = Instantiate(ability, slot4.transform);
            obj.SetActive(true);
            num4.SetActive(true);
        }
    }
}
