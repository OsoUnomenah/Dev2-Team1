using UnityEngine;
using System.Collections;

public class WarehouseInteract : MonoBehaviour, IInteract
{
    [SerializeField] private Transform garageDoor;
    
    [SerializeField] private float doorSpeed;
    [SerializeField] private float heightFinal;
    [SerializeField] private float heightStart;

    [SerializeField] private Light hoverLight;

    private bool doorOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        doorOpen = false;
        hoverLight.enabled = false;
    }

    public void Interact()
    {
        

        StartCoroutine(OpenOrClose());

    }

    private IEnumerator OpenOrClose()
    {
        if (!doorOpen)
        {
            while (garageDoor.position.y < heightFinal)
            {
                garageDoor.position += Vector3.up * doorSpeed * Time.deltaTime;

                yield return null;
            }

            doorOpen = true;
        } 
        else if (doorOpen)
        {
            while (garageDoor.position.y > heightStart)
            {
                garageDoor.position -= Vector3.up * doorSpeed * Time.deltaTime;

                yield return null;
            }

            doorOpen = false;
        }
    }

    public void OnHoverEnter()
    {
        hoverLight.enabled = true;
        Debug.Log("Garage Door Opener");
    }

    public void OnHoverExit()
    {
        hoverLight.enabled = false;
    }
}
