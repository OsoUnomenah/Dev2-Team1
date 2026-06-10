using System.Collections;
using UnityEngine;

public class DoorInteract : MonoBehaviour, IInteract
{

    [Header("Door Settings")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] float openAngle; //how far door opens
    [SerializeField] float openSpeed; //how fast door opens/closes

    [Header("Highlight Settings")]
    [SerializeField] private Renderer model;
    [SerializeField] private Material highlight;


    private Material materialOrig;
    private bool isOpen;
    private bool isMoving; //stops door from being spammed when its moving
    private Quaternion closedRotation;
    private Quaternion openRotation;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(doorTransform == null) //if nothing is aissnged, use the object this script is attached too
        {
            doorTransform = transform;
        }

        //save the doors starting rotation as close position
        closedRotation = doorTransform.rotation;

        //create the open rotation based on the open angle
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        if(model != null )
        {
            materialOrig = model.material;
        }


    }


    public void Interact()
    {

        //do not start another interact while already moving
        if (isMoving)
            return;


        //pick the target rotation based on door state
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        StartCoroutine(RotateDoor(targetRotation));

        isOpen = !isOpen;
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;

        //keep rotating until the door is close enogugh to target rotation
        while(Quaternion.Angle(doorTransform.rotation, targetRotation) > 0.1f)
        {
            doorTransform.rotation = Quaternion.Slerp(doorTransform.rotation, targetRotation, openSpeed * Time.deltaTime); //slerp like lerp just on spherical edge rather than staright line

            yield return null;

        }

        //snap door closed perfect at the end of rotation
        doorTransform.rotation = targetRotation;

        isMoving = false;

        
    }

    public void OnHoverEnter()
    {
       if(model != null && highlight != null)
        {
            model.material = highlight;
        }
    }

    public void OnHoverExit()
    {
        if (model != null && materialOrig != null)
        {
            model.material = materialOrig;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOpen && other.CompareTag("Enemy"))
        {
            Quaternion targetRotation = openRotation;
            StartCoroutine(RotateDoor(targetRotation));

            isOpen = true;
        }
    }
}
