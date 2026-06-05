using UnityEngine;

public class Players : MonoBehaviour
{
    [SerializeField] private Transform interactorSource;
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask ignoreSource;

    private IInteract currentInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HoverHandler();
    }

    private void HoverHandler()
    {
        RaycastHit hit;
        IInteract newObj = null;

        if (Physics.Raycast(interactorSource.position, interactorSource.forward, out hit, interactRange, ~ignoreSource))
        {
            newObj = hit.collider.GetComponentInParent<IInteract>();
        }

        if (currentInteractable != null && (Object)currentInteractable == null)
        {
            currentInteractable = null;
        }

        if (newObj != currentInteractable)
        {
            if (currentInteractable != null && (Object)currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
            }

            currentInteractable = newObj;

            if (currentInteractable != null && (Object)currentInteractable != null)
            {
                currentInteractable.OnHoverEnter();
            }
        }

    }
}
