using TMPro;
using UnityEngine;

public class Players : MonoBehaviour
{
    [SerializeField] private Transform interactorSource;
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask ignoreSource;
    [SerializeField] private TextMeshProUGUI interactText; //"Press E to interact" when hovering over interactable objects

    private IInteract currentInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }


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

                if (interactText != null)
                {
                    interactText.gameObject.SetActive(false);
                }
            }

            currentInteractable = newObj;

            if (currentInteractable != null && (Object)currentInteractable != null)
            {
                currentInteractable.OnHoverEnter();

                if (interactText != null && currentInteractable is not IDamage) //so interact text will not pop up when hovering enemy
                {
                    interactText.gameObject.SetActive(true);
                }
            }
        }

    }
}
