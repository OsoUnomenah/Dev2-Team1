using UnityEngine;
using System.Collections;

public class WarehouseInteract : MonoBehaviour, IInteract
{
    [SerializeField] private GameObject door;

    [SerializeField] private float doorSpeed;
    [SerializeField] private float heightFinal;
    [SerializeField] private float heightStart;

    [SerializeField] private Light hoverLight;

    [Header("Audio")]
    [SerializeField] private BaseSoundSO _raise;

    private bool doorOpen;
    private bool doorMoving;

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
        AudioManager.instance.PlaySoundAtPosition(_raise, door);
        if (!doorOpen && !doorMoving)
        {
 
            while (door.transform.position.y < heightFinal)
            {
                door.transform.position += Vector3.up * doorSpeed * Time.deltaTime;
                yield return null;
                doorMoving = true;
            }

            doorOpen = true;
        }
        else if (doorOpen && !doorMoving)
        {
 
            while (door.transform.position.y > heightStart)
            {
                door.transform.position -= Vector3.up * doorSpeed * Time.deltaTime;
                yield return null;
                doorMoving = true;
            }

            doorOpen = false;
        }

        doorMoving = false;
    }

    public void OnHoverEnter()
    {
        hoverLight.enabled = true;
        Debug.Log("Door Button");
    }

    public void OnHoverExit()
    {
        hoverLight.enabled = false;
    }
}
