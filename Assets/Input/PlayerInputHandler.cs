using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

//Steps to use
//1. Setup bindings in Unity Editor using PlayerInputHandler ActionMap
//2. Add the action in class section (see jumpAction for example)
//3. if necessary create bool for triggering
//4. create subscribe/unsubscribe in methods Enable and Disabled
//5. create logic for perform and cancelled methods(will need to make methods)
// extra note if turnondebug is set to true will show debug messages 



public class PlayerInputHandler : MonoBehaviour, IDamage
{
    [Header("In-Game Debug")]
    [SerializeField] bool turnOnDebug;

    [Header("References")]
    [SerializeField] private CharacterController characterController; // Reference to characterController to be seen in Unity
    [SerializeField] private PlayerInputHandler playerInputHandler;  // Reference to playerInputHandler to be exposed in Unity
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerWeaponManager weaponManager;

    [Header("Movement Config")]
    [Range(3.0f, 20.0f)][SerializeField] private float walkSpeed = 3.0f; // How fast player moves
    [Range(2.0f, 5.0f)][SerializeField] private float sprintMultiplier = 2.0f;
    private Vector3 currentMovement;


    [Header("Rotation Config")]
    [Range(0.1f, 5.0f)][SerializeField] private float mouseSensitivity = 0.5f;
    [Range(1.0f, 10.0f)][SerializeField] private float gamepadSensitivity = 1.5f;
    [SerializeField] private float verticalViewRange = 80f;
    private float verticalRotation;


    [Header("Inventory Config")]
    [SerializeField] private string selected;



    [Header("Interact Config")]
    [SerializeField] public Transform interactorSource;
    [SerializeField] public float interactRange;
    [SerializeField] public LayerMask ignoreSource;

    [SerializeField] public int HP;




    // [Header("Combat Settings")] //Changed these to be exclusively tied to the WeaponManager values. 


    private PlayerActions playerActions; // Reference to the generated input actions class

    private InputAction moveAction;
    private InputAction rotateAction;

    private InputAction jumpAction; // Reference to the specific input action for jumping
    private InputAction sprintAction;

    private InputAction interactAction;
    private InputAction shootAction;


    [Header("GlobalVariables")]
    public bool SprintTriggered { get; private set; }
    public Vector2 MovementVector { get; private set; }
    public Vector2 RotateVector { get; private set; }


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        weaponManager = FindAnyObjectByType<PlayerWeaponManager>(); //same edit as WeaponPickUp.cs
    }

    void Update()
    {
        if (weaponManager == null)
        {
            weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
            if (weaponManager == null) return;
        }

        weaponManager.Timer += Time.deltaTime;
        HandleMovement();
        HandleRotation();
        ApplyMovement();
        HandleJumping();
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            HP = 0;
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject, 1.5f);
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

        rotateAction.performed -= OnRotatePerformed;
        rotateAction.canceled -= OnRotateCanceled;

        jumpAction.performed -= OnJumpPerformed; // Unsubscribe from the performed event of the jump action
        jumpAction.canceled -= OnJumpCanceled; // Unsubscribe from the canceled event of the jump action

        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

        shootAction.performed -= OnShootPerformed;
        shootAction.canceled -= OnShootCanceled;

    }

    private void HandleRotation()
    {
        // Detect current input device and apply appropriate sensitivity
        float currentSensitivity = GetCurrentSensitivity();

        float mouseXRotation = playerInputHandler.RotateVector.x * currentSensitivity;
        float mouseYRotation = playerInputHandler.RotateVector.y * currentSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

    private float GetCurrentSensitivity()
    {
        // Check which device was last used for the rotate action
        if (rotateAction.activeControl != null)
        {
            var device = rotateAction.activeControl.device;

            // Check if it's a gamepad
            if (device is Gamepad)
            {
                return gamepadSensitivity;
            }
        }

        // Default to mouse sensitivity for mouse/keyboard
        return mouseSensitivity;
    }

    private void ApplyVerticalRotation(float mouseYRotation)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -verticalViewRange, verticalViewRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void ApplyHorizontalRotation(float mouseXRotation)
    {
        characterController.transform.Rotate(0, mouseXRotation, 0);
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        float currentSpeed = (walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1.0f)); // If sprint is triggered, multiply walk speed by sprint multiplier 
        currentMovement.x = worldDirection.x * currentSpeed;
        currentMovement.z = worldDirection.z * currentSpeed;
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementVector.x, 0, playerInputHandler.MovementVector.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);

        return worldDirection.normalized;
    }


    private void ApplyMovement()
    {
        characterController.Move(currentMovement * Time.deltaTime);
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

    [Header("Jump Config")]
    [Range(1.0f, 10.0f)][SerializeField] private float jumpForce = 5.0f;
    [Range(1.0f, 3.0f)][SerializeField] private float gravityMultiplier = 1.0f;
    [Range(1, 3)][SerializeField] private int jumpMax;
    public bool JumpTriggered { get; private set; }
    public int jumpCount;
    public bool canJump;


    private void HandleJumping()
    {
        if (jumpCount >= jumpMax)
        {
            canJump = false;
        }

        if (characterController.isGrounded)
        {
            canJump = true;
            jumpCount = 0;
        }

        if (JumpTriggered)
        {
            currentMovement.y = jumpForce;
            jumpCount++;
            JumpTriggered = false;
        }

        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }
    IEnumerator PlaySound(BaseSoundSO sound)
    {
        if (sound != null)
        {
            GameObject soundObject = new GameObject("Temp Audio Source");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.Play();
            yield return new WaitForSeconds(0.3f);
            Destroy(soundObject);
        }
    }

    [SerializeField] private BaseSoundSO _jump;
    [SerializeField] private BaseSoundSO _sprint;
    [SerializeField] private BaseSoundSO _shoot;




    public void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (canJump)
        {
            JumpTriggered = true;
            StartCoroutine(PlaySound(_jump));

            if (turnOnDebug)
            {

                Debug.Log("Jump Performed"); // Log a message to the console when the jump action is performed
            }
        }

    }

    public void OnJumpCanceled(InputAction.CallbackContext context)
    {
        JumpTriggered = false;


        if (turnOnDebug)
        {
            Debug.Log("Jump Canceled!"); // Log a message to the console when the jump action is canceled
        }
    }

    public void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        SprintTriggered = true;
        StartCoroutine(PlaySound(_sprint));



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
        Debug.Log("InteractorSource: " + interactorSource);
        Debug.Log("WeaponManager: " + weaponManager);
        RaycastHit hit;
        if (Physics.Raycast(interactorSource.position, interactorSource.forward, out hit, interactRange, ~ignoreSource))
        {
            Debug.Log(hit.collider.name);

            IInteract iAct = hit.collider.GetComponentInParent<IInteract>(); //added "GetComponentInParent" to check object as well as parent not just object
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
        StartCoroutine(PlaySound(_shoot));

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, weaponManager.Range, ~ignoreSource))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null && weaponManager.Damage != 0)
            {
                dmg.takeDamage(weaponManager.Damage);

            }
        }

        if (turnOnDebug)
        {
            Debug.Log("ShotFired!");
        }
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {


    }
}
