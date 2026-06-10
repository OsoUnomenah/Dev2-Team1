using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

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
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerWeaponManager weaponManager;
    [SerializeField] private GameObject playerStatHandler;

    [Header("Movement Config")]
    [Range(3.0f, 20.0f)][SerializeField] private float walkSpeed = 3.0f;
    [Range(1.0f, 5.0f)][SerializeField] private float sprintMultiplier = 2.0f;
    [Range(10.0f, 80.0f)][SerializeField] private float acceleration = 10.0f;
    private Vector3 currentMovement;
    private float currentSpeed = 0f;

    [Header("Rotation Config")]
    [Range(0.1f, 5.0f)][SerializeField] private float mouseSensitivity = 0.5f;
    [Range(1.0f, 10.0f)][SerializeField] private float gamepadSensitivity = 1.5f;
    [SerializeField] private float verticalViewRange = 80f;
    private float verticalRotation;
    float recoil;
    float timer;

    private bool isReloading;
    private float reloadTimer;

    [Header("Inventory Config")]
    [SerializeField] private string selected;

    [Header("Interact Config")]
    [SerializeField] public Transform interactorSource;
    [SerializeField] public float interactRange;
    [SerializeField] public LayerMask ignoreSource;

    [SerializeField] public int HP;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _shoot;
    [SerializeField] BaseSoundSO _footsteps;
    [Range(.4f, 1f)][SerializeField] private float footstepBaseInterval;
    [Range(.4f, 1f)][SerializeField] private float footstepSprintInterval = 0.5f;

    private float footstepTimer;



    // [Header("Combat Settings")] //Changed these to be exclusively tied to the WeaponManager values. 


    private PlayerActions playerActions; // Reference to the generated input actions class

    private InputAction moveAction;
    private InputAction rotateAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction pauseAction;

    [Header("GlobalVariables")]
    public bool SprintTriggered { get; private set; }
    public Vector2 MovementVector { get; private set; }
    public Vector2 RotateVector { get; private set; }

    void Awake()
    {
        playerActions = new PlayerActions();

        moveAction = playerActions.PlayerInput.Movement;
        rotateAction = playerActions.PlayerInput.Rotate;

        jumpAction = playerActions.PlayerInput.Jump;
        sprintAction = playerActions.PlayerInput.Sprint;

        interactAction = playerActions.PlayerInput.Interact;
        shootAction = playerActions.PlayerInput.Shoot;

        pauseAction = playerActions.PlayerInput.Pause;
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
        playerStatHandler = GameObject.FindGameObjectWithTag("PlayerStatHandler");
    }

    void Update()
    {
        if (weaponManager == null)
        {
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
            if (weaponManager == null) return;
        }

        HandleSprintInput();
        HandleMovement();
        HandleRotation();
        ApplyMovement();
        HandleFootsteps();
        HandleJumping();
        HandleReloadInput();
        ShootTimer();
        IfReload();
    }

    public void takeDamage(int amount)
    {
        if (playerStatHandler == null)
        {
            return;
        }

        StatHandler stats = playerStatHandler.GetComponent<StatHandler>();

        if (stats == null)
        {
            return;
        }

        int defenseBonus = Mathf.RoundToInt(stats.modDefense);

        // Defense reduces incoming damage.
        // Minimum damage is 1 so enemies can still hurt the player.
        int finalDamage = Mathf.Max(1, amount - defenseBonus);

        stats.currentHealth -= finalDamage;

        if (turnOnDebug)
        {
            Debug.Log("Enemy Damage: " + amount + " - Defense: " + defenseBonus + " = " + finalDamage);
        }

        if (stats.currentHealth <= 0)
        {
            gameManager.instance.youLose();
        }
    }

    void OnEnable()
    {
        playerActions.Enable();

        moveAction.performed += OnMovementPerformed;
        moveAction.canceled += OnMovementCanceled;

        rotateAction.performed += OnRotatePerformed;
        rotateAction.canceled += OnRotateCanceled;

        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;

        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;

        interactAction.performed += OnInteractPerformed;
        interactAction.canceled += OnInteractCanceled;

        shootAction.performed += OnShootPerformed;
        shootAction.canceled += OnShootCanceled;

        pauseAction.performed += OnPausePerformed;
        pauseAction.canceled += OnPauseCanceled;
    }

    void OnDisable()
    {
        playerActions.Disable();

        moveAction.performed -= OnMovementPerformed;
        moveAction.canceled -= OnMovementCanceled;

        rotateAction.performed -= OnRotatePerformed;
        rotateAction.canceled -= OnRotateCanceled;

        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

        interactAction.performed -= OnInteractPerformed;
        interactAction.canceled -= OnInteractCanceled;

        shootAction.performed -= OnShootPerformed;
        shootAction.canceled -= OnShootCanceled;

        pauseAction.performed -= OnPausePerformed;
        pauseAction.canceled -= OnPauseCanceled;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        gameManager.instance.PauseGame();
    }

    private void OnPauseCanceled(InputAction.CallbackContext context)
    {
        // cancel logic for button release if needed
    }

    private void HandleRotation()
    {
        if (gameManager.instance.isLevelingUp)
        {
            RotateVector = Vector2.zero;
            return;
        }

        float currentSensitivity = GetCurrentSensitivity();

        float mouseXRotation = playerInputHandler.RotateVector.x * currentSensitivity;
        float mouseYRotation = playerInputHandler.RotateVector.y * currentSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

    private float GetCurrentSensitivity()
    {
        if (rotateAction.activeControl != null)
        {
            var device = rotateAction.activeControl.device;

            if (device is Gamepad)
            {
                return gamepadSensitivity;
            }
        }

        return mouseSensitivity;
    }

    private void ApplyVerticalRotation(float mouseYRotation)
    {
        if (!gameManager.instance.isPaused)
        {
            recoil = Mathf.Lerp(recoil, 0f, Time.deltaTime * 10f);
            verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -verticalViewRange, verticalViewRange);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            verticalRotation -= recoil;
        }
    }

    private void ApplyHorizontalRotation(float mouseXRotation)
    {
        characterController.transform.Rotate(0, mouseXRotation, 0);
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();

        float targetSpeed = gameManager.instance.SprintTriggered
            ? walkSpeed * sprintMultiplier
            : walkSpeed;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

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
            Debug.Log(MovementVector);
        }
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        MovementVector = Vector2.zero;

        if (turnOnDebug)
        {
            Debug.Log(MovementVector);
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        if (!gameManager.instance.isPaused)
        {
            RotateVector = context.ReadValue<Vector2>();

            if (turnOnDebug)
            {
                Debug.Log(RotateVector);
            }
        }
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        RotateVector = Vector2.zero;

        if (turnOnDebug)
        {
            Debug.Log(RotateVector);
        }
    }

    [Header("Jump Config")]
    [Range(1.0f, 10.0f)][SerializeField] private float jumpForce = 5.0f;
    [Range(1.0f, 3.0f)][SerializeField] private float gravityMultiplier = 1.0f;
    [Range(1, 5)][SerializeField] private int jumpMax;
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
            AudioManager.instance.PlayFootsteps(_footsteps, gameManager.instance.player);
            jumpCount++;
            JumpTriggered = false;
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    public void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (canJump)
        {
            JumpTriggered = true;

            if (turnOnDebug)
            {
                Debug.Log("Jump Performed");
            }
        }
    }

    public void OnJumpCanceled(InputAction.CallbackContext context)
    {
        JumpTriggered = false;

        if (turnOnDebug)
        {
            Debug.Log("Jump Canceled!");
        }
    }

    public void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        if (gameManager.instance.canSprint)
        {
            gameManager.instance.SprintTriggered = true;
        }

        if (turnOnDebug)
        {
            Debug.Log("Sprinting!");
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        gameManager.instance.SprintTriggered = false;
        gameManager.instance.isSprinting = false;

        if (turnOnDebug)
        {
            Debug.Log("Sprinting Canceled!");
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

            IInteract iAct = hit.collider.GetComponentInParent<IInteract>();
            if (iAct != null)
            {
                iAct.Interact();
            }
        }

        Debug.DrawRay(interactorSource.position, interactorSource.forward * interactRange, Color.green);

        if (turnOnDebug)
        {
            Debug.Log("Interact Started!");
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (turnOnDebug)
        {
            Debug.Log("Stopped Interacting!");
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (weaponManager == null || weaponManager.Damage <= 0 || weaponManager.Range <= 0)
        {
            return;
        }

        if (gameManager.instance.isReloading)
        {
            Debug.Log("Cannot shoot while reloading.");
            return;
        }

        if (weaponManager.Ammo <= 0)
        {
            
            StartReload();
            return;
        }

        if(gameManager.instance.canShoot == true)
        { 
            recoil = gameManager.instance.recoil; 
        }
        else
        {
            recoil = 0;
        }

        if (!gameManager.instance.isPaused && !gameManager.instance.isLevelingUp && gameManager.instance.canShoot == true)
        {
            timer = 0;
            gameManager.instance.canShoot = false;

            AudioManager.instance.PlaySound(_shoot);

            weaponManager.Ammo--;

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, weaponManager.Range, ~ignoreSource))
            {
                Debug.Log(hit.collider.name);

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (dmg != null && weaponManager.Damage != 0)
                {
                    int bonusDamage = 0;

                    if (playerStatHandler != null)
                    {
                        StatHandler stats = playerStatHandler.GetComponent<StatHandler>();

                        if (stats != null)
                        {
                            bonusDamage = Mathf.RoundToInt(stats.modDamage);
                        }
                    }

                    int finalDamage = weaponManager.Damage + bonusDamage;

                    dmg.takeDamage(finalDamage);

                    if (turnOnDebug)
                    {
                        Debug.Log("Weapon Damage: " + weaponManager.Damage + " + Bonus Damage: " + bonusDamage + " = " + finalDamage);
                    }
                }
            }

            if (weaponManager.Ammo <= 0)
            {
                StartReload();
            }

            if (turnOnDebug)
            {
                Debug.Log("ShotFired!");
            }
        }
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        // cancel logic for button release if needed
    }

    private void ShootTimer()
    {
        if (weaponManager == null)
        {
            return;
        }

        if (gameManager.instance.isReloading)
        {            
            reloadTimer += Time.deltaTime;
            gameManager.instance.canShoot = false;

            if (reloadTimer >= weaponManager.AmmoTimer)
            {
                weaponManager.Ammo = weaponManager.MaxAmmo;
                gameManager.instance.isReloading = false;
                reloadTimer = 0;
                gameManager.instance.canShoot = true;


                Debug.Log("Reload complete!");
            }
            
            return;
        }

        timer += Time.deltaTime;

        if (timer >= weaponManager.Timer)
        {
            gameManager.instance.canShoot = true;
        }
    }

    private void HandleReloadInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if(Keyboard.current.rKey.wasPressedThisFrame && reloadTimer < weaponManager.AmmoTimer)
        {
            weaponManager.Ammo = 0;
            gameManager.instance.isReloading = true;
            StartReload();
        }
    }

    private void IfReload()
    {
        if (gameManager.instance.isReloading)
        {            
            gameManager.instance.reloadMax = weaponManager.AmmoTimer;
            gameManager.instance.reloadTime = reloadTimer;
        }
        
    }
    private void StartReload()
    {
        if (weaponManager == null)
        {
            return;
        }

        if (weaponManager.MaxAmmo <= 0)
        {
            return;
        }        

        if (isReloading)
        {
            return;
        }

        isReloading = true;
        reloadTimer = 0;
        gameManager.instance.canShoot = false;

        

        Debug.Log("Reloading...");
    }

    private void HandleFootsteps()
    {
        if (!moveAction.IsPressed())
        {
            footstepTimer = 0;
            return;
        }

        if (characterController.isGrounded)
        {
            footstepTimer += Time.deltaTime;

            float interval = gameManager.instance.SprintTriggered
                ? footstepBaseInterval * footstepSprintInterval
                : footstepBaseInterval;

            if (footstepTimer >= interval)
            {
                AudioManager.instance.PlayFootsteps(_footsteps, gameManager.instance.player);
                footstepTimer = 0;
            }
        }
    }

    private void HandleSprintInput()
    {
        if (gameManager.instance == null || sprintAction == null)
        {
            return;
        }

        bool sprintHeld = sprintAction.IsPressed();

        if (sprintHeld && gameManager.instance.canSprint)
        {
            gameManager.instance.SprintTriggered = true;
        }
        else
        {
            gameManager.instance.SprintTriggered = false;

            if (!sprintHeld)
            {
                gameManager.instance.isSprinting = false;
            }
        }
    }
}