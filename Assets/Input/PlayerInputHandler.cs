using System;
using UnityEngine;
using UnityEngine.InputSystem;

//Steps to use
//1. Setup bindings in Unity Editor using PlayerInputHandler ActionMap
//2. Add the action in class section (see jumpAction for example)
//3. if necessary create bool for triggering
//4. create subscribe/unsubscribe in methods Enable and Disabled
//5. create logic for perform and cancelled methods(will need to make methods)
// extra note if turnondebug is set to true will show debug messages 



public class PlayerInputHandler : MonoBehaviour
{
    private PlayerWeaponManager weaponManager;
    [Header("Debug")]
    [SerializeField] bool turnOnDebug;

    [Header("Interact Config")]
    [SerializeField] public Transform interactorSource;
    [SerializeField] public float interactRange;
    [SerializeField] public LayerMask ignoreSource;

    [Header("Combat Settings")] //Changed these to be exclusively tied to the WeaponManager values. 
    

    private PlayerActions playerActions; // Reference to the generated input actions class

    private InputAction moveAction;
    private InputAction rotateAction;

    private InputAction jumpAction; // Reference to the specific input action for jumping
    private InputAction sprintAction;

    private InputAction interactAction;
    private InputAction shootAction;



    public bool JumpTriggered { get; private set; } // Property boolean to track if the jump action was triggered
    public bool SprintTriggered { get; private set; }
    public Vector2 MovementVector { get; private set; }
    public Vector2 RotateVector { get; private set; }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
    }

    void Update()
    {
        weaponManager.Timer += Time.deltaTime;
    }


    void Awake() // Initialize the input actions and get references to specific actions
    {

        playerActions = new PlayerActions(); // Create an instance of the generated input actions class

        moveAction = playerActions.PlayerInput.Movement;
        rotateAction = playerActions.PlayerInput.Rotate;
        
        jumpAction = playerActions.PlayerInput.Jump; // Get the specific input action for jumping from the generated class
        sprintAction = playerActions.PlayerInput.Sprint;

        interactAction = playerActions.PlayerInput.Interact;
        shootAction = playerActions.PlayerInput.Shoot;
    }

    void OnEnable()
    {
        playerActions.Enable(); // Enable the input actions when the script is enabled

        moveAction.performed += OnMovementPerformed;
        moveAction.canceled += OnMovementCanceled;

        rotateAction.performed += OnRotatePerformed;
        rotateAction.canceled += OnRotateCanceled;

        jumpAction.performed += OnJumpPerformed; // Subscribe to the performed event of the jump action
        jumpAction.canceled += OnJumpCanceled; // Subscribe to the canceled event of the jump action

        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;

        interactAction.performed += OnInteractPerformed;
        interactAction.canceled += OnInteractCanceled;

        shootAction.performed += OnShootPerformed;
        shootAction.canceled += OnShootCanceled;
    }


    void OnDisable()
    {
        playerActions.Disable(); // Disable the input actions when the script is disabled

        moveAction.performed -= OnMovementPerformed;
        moveAction.canceled -= OnMovementCanceled;

        rotateAction.performed += OnRotatePerformed;
        rotateAction.canceled += OnRotateCanceled;

        jumpAction.performed -= OnJumpPerformed; // Unsubscribe from the performed event of the jump action
        jumpAction.canceled -= OnJumpCanceled; // Unsubscribe from the canceled event of the jump action

        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

        shootAction.performed -= OnShootPerformed;
        shootAction.canceled -= OnShootCanceled;

    }


    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        MovementVector = context.ReadValue<Vector2>();

        if (turnOnDebug)
        {
            Debug.Log(MovementVector); // Log a message to the console when the move action is performed
        }
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        MovementVector = Vector2.zero;

        if (turnOnDebug)
        {
            Debug.Log(MovementVector); // Log a message to the console when the move action is performed
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        RotateVector = context.ReadValue<Vector2>();

        if (turnOnDebug)
        {
            Debug.Log(RotateVector); // Log a message to the console when the move action is performed
        }
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        RotateVector = Vector2.zero;

        if (turnOnDebug)
        {
            Debug.Log(RotateVector); // Log a message to the console when the move action is performed
        }
    }


    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpTriggered = true;

        if (turnOnDebug)
        {
            Debug.Log("Jumping!"); // Log a message to the console when the jump action is performed
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        JumpTriggered = false;

        if (turnOnDebug)
        {
            Debug.Log("Jump Canceled!"); // Log a message to the console when the jump action is canceled
        }
    }

    private void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        SprintTriggered = true;

        if (turnOnDebug)
        {
            Debug.Log("Sprinting!"); // Log a message to the console when the sprint action is performed
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        SprintTriggered = false;

        if (turnOnDebug)
        {
            Debug.Log("Sprinting Canceled!"); // Log a message to the console when the sprint action is performed
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {

        if (turnOnDebug)
        {
            Debug.Log("Stopped Interacting!"); // Log a message to the console when the interact action is canceled
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        if (Physics.Raycast(interactorSource.position, interactorSource.forward, out hit, interactRange, ~ignoreSource))
        {
            Debug.Log(hit.collider.name);

            IInteract iAct = hit.collider.GetComponent<IInteract>();
            if (iAct != null)
            {
                iAct.Interact();
            }
        }
        Debug.DrawRay(interactorSource.position, interactorSource.forward * interactRange, Color.green);

        if (turnOnDebug)
        {
            Debug.Log("Interact Started!"); // Log a message to the console when the interact action is performed
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        weaponManager.Timer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, weaponManager.Range, ~ignoreSource))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(weaponManager.Damage);

            }
        }
        if(turnOnDebug)
        {
            Debug.Log("ShotFired!");
        }
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {

        
    }

}
