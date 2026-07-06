using UnityEngine;
using System.Collections;


public class ChestInteract : MonoBehaviour, IInteract
{
    [Header("Chest Settings")]
    [SerializeField] private Transform lidTransform; //chest lid that rotates open
    [SerializeField] float openAngle; //how far the lid opens
    [SerializeField] float openSpeed; //how fast the lid opens 

    [Header("Highlight Settings")]
    [SerializeField] private Renderer model;
    [SerializeField] private Material highlight;


    private Material materialOrig; //stores OG material
    private bool isOpen; //stops chest from opening more than once
    private bool isMoving; //stops spam interaction
    private Quaternion closedRotation; //lids starting rotation
    private Quaternion openRotation; //lids open rotation





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(lidTransform == null)
            lidTransform = transform;


        closedRotation = lidTransform.rotation; //saves lids starting postion as closed rotation

        openRotation = closedRotation * Quaternion.Euler(openAngle, 0, 0); //create open rotation from closed rotation

        if(model != null)
        {
            materialOrig = model.material; //save original material so hover can reset it
        }



    }

    

    public void Interact()
    {
        if (isOpen || isMoving) //do nothing if chest is already open or currently moving
            return;

        StartCoroutine(OpenChest()); //start opening chest smooth

        isOpen = true;
    }

    private IEnumerator OpenChest()
    {
        isMoving = true;

        while(Quaternion.Angle(lidTransform.rotation, openRotation) > 0.1f) //same opening mechanic as door
        {
            lidTransform.rotation = Quaternion.Slerp(lidTransform.rotation, openRotation, openSpeed * Time.deltaTime); //smooth open

            yield return null;
        }

        lidTransform.rotation = openRotation; //snap perfectly to open rotation at end

        isMoving = false;

        Debug.Log("Chest Opened!");
    }

    public void OnHoverEnter()
    {
        if (model != null && highlight != null)
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
}
