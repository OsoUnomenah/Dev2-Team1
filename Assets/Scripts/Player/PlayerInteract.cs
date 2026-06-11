using TMPro;
using UnityEngine;

public class Players : MonoBehaviour, IOpen
{
    [SerializeField] private Transform interactorSource;
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask ignoreSource;
    [SerializeField] private TextMeshProUGUI interactText; //"Press E to interact" when hovering over interactable objects
    [SerializeField] private PlayerWeaponManager weaponManager;
       
    private IInteract currentInteractable;

    RaycastHit hit;
    IInteract newObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }

        weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
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

        if (Physics.Raycast(interactorSource.position, interactorSource.forward, out hit, weaponManager.Range, ~ignoreSource))
        {
            float distance = hit.distance;

            IInteract interactable = hit.collider.GetComponentInParent<IInteract>();
            
            if (hit.collider.CompareTag("Enemy"))
            {
                newObj = hit.collider.GetComponentInParent<IInteract>();
            }
            else
            {
                if (distance <= interactRange)
                {
                    newObj = interactable;
                }
            }            
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
