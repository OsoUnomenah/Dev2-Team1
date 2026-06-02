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
    [SerializeField] bool turnOnDebug;

    private PlayerActions playerActions; // Reference to the generated input actions class

    private InputAction moveAction;

    private InputAction jumpAction; // Reference to the specific input action for jumping
    private InputAction sprintAction; 

    public bool JumpTriggered { get; private set; } // Property boolean to track if the jump action was triggered
    public bool SprintTriggered { get; private set; }
    public Vector2 MovementVector { get; private set; }



    void Awake() // Initialize the input actions and get references to specific actions
    {
        
        playerActions = new PlayerActions(); // Create an instance of the generated input actions class

        moveAction = playerActions.PlayerInput.Movement;
        
        jumpAction = playerActions.PlayerInput.Jump; // Get the specific input action for jumping from the generated class
        sprintAction = playerActions.PlayerInput.Sprint;

    }

    void OnEnable()
    {
        playerActions.Enable(); // Enable the input actions when the script is enabled

        moveAction.performed += OnMovementPerformed;
        moveAction.canceled += OnMovementCanceled;

        jumpAction.performed += OnJumpPerformed; // Subscribe to the performed event of the jump action
        jumpAction.canceled += OnJumpCanceled; // Subscribe to the canceled event of the jump action

        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
    }

    void OnDisable()
    {
        playerActions.Disable(); // Disable the input actions when the script is disabled

        moveAction.performed -= OnMovementPerformed;
        moveAction.canceled -= OnMovementCanceled;

        jumpAction.performed -= OnJumpPerformed; // Unsubscribe from the performed event of the jump action
        jumpAction.canceled -= OnJumpCanceled; // Unsubscribe from the canceled event of the jump action

        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

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

}
