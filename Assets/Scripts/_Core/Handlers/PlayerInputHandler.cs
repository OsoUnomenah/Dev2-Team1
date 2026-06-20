using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using System.Collections.Generic;

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

    [Header("Movement Config")]
    [Range(3.0f, 20.0f)][SerializeField] private float walkSpeed = 3.0f;
    [Range(3.0f, 20.0f)][SerializeField] private float sprintSpeed = 3.0f;
    [Range(1.0f, 5.0f)][SerializeField] private float sprintMultiplier = 2.0f;
    [Range(10.0f, 80.0f)][SerializeField] private float acceleration = 10.0f;
    public Vector3 currentMovement;
    public float currentSpeed = 0f;

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

    [Header("Audio")]
    [SerializeField] BaseSoundSO _shoot;
    [SerializeField] BaseSoundSO _footsteps;
    [Range(.4f, 1f)][SerializeField] private float footstepBaseInterval;
    [Range(.4f, 1f)][SerializeField] private float footstepSprintInterval = 0.5f;

    private float footstepTimer;
    private StatHandler playerStats;


    // [Header("Combat Settings")] //Changed these to be exclusively tied to the WeaponManager values. 


    private PlayerActions playerActions; // Reference to the generated input actions class

    private InputAction moveAction;
    private InputAction rotateAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction adsAction;
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
        reloadAction = playerActions.PlayerInput.Reload;
        adsAction = playerActions.PlayerInput.ADS;

        pauseAction = playerActions.PlayerInput.Pause;

    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;   
    }

    void Update()
    {
       
        HandleMovement();
        HandleRotation();
        ApplyMovement();
        HandleFootsteps();
        HandleJumping();
        ShootTimer();
        HandleReload();
    }

    public void takeDamage(int amount)
    {

       
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
        reloadAction.performed += OnReloadPerformed;
        reloadAction.canceled += OnReloadCanceled;
        adsAction.performed += OnADSPerformed;
        adsAction.canceled += OnADSCanceled;

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

        float mouseXRotation = RotateVector.x * currentSensitivity;
        float mouseYRotation = RotateVector.y * currentSensitivity;

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
            gameManager.instance.playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            verticalRotation -= recoil;
        }
    }

    private void ApplyHorizontalRotation(float mouseXRotation)
    {
        gameManager.instance.characterController.transform.Rotate(0, mouseXRotation, 0);
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();

        float targetSpeed = gameManager.instance.SprintTriggered ? sprintSpeed * sprintMultiplier : walkSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
        currentMovement.x = worldDirection.x * currentSpeed;
        currentMovement.z = worldDirection.z * currentSpeed;
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(MovementVector.x, 0, MovementVector.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);

        return worldDirection.normalized;
    }

    private void ApplyMovement()
    {
        gameManager.instance.characterController.Move(currentMovement * Time.deltaTime);
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

        if (gameManager.instance.characterController.isGrounded)
        {
            canJump = true;
            jumpCount = 0;
            jumpMax = gameManager.instance.playerStatHandler.modJumps;
            currentMovement.y = 0;
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
        if (gameManager.instance.canSprint && gameManager.instance.characterController.isGrounded)
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
        Debug.Log("WeaponManager: " + gameManager.instance.playerWeaponManager);

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

        if (gameManager.instance.gameDebug)
        {
            Debug.Log("Interact Started!");
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (gameManager.instance.gameDebug)
        {
            Debug.Log("Stopped Interacting!");
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (gameManager.instance.playerWeaponManager == null 
            || gameManager.instance.playerWeaponManager.Damage <= 0 
            || gameManager.instance.playerWeaponManager.Range <= 0)
        {
            return;
        }

        if (gameManager.instance.isReloading)
        {
            Debug.Log("Cannot shoot while reloading.");
            return;
        }

        if (gameManager.instance.playerWeaponManager.Ammo <= 0)
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

            gameManager.instance.playerWeaponManager.Ammo--;

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, gameManager.instance.playerWeaponManager.Range, ~ignoreSource))
            {
                Debug.Log(hit.collider.name);

                IDamage dmg = hit.collider.GetComponentInChildren<IDamage>();

                if (dmg != null && gameManager.instance.playerWeaponManager.Damage != 0)
                {
                    int bonusDamage = 0;

                    
                        StatHandler stats = gameManager.instance.playerStatHandler;

                        if (stats != null)
                        {
                            bonusDamage = Mathf.RoundToInt(stats.modDamage);
                        }

                    int finalDamage = gameManager.instance.playerWeaponManager.Damage + bonusDamage;

                    dmg.takeDamage(finalDamage);

                    if (turnOnDebug)
                    {
                        Debug.Log("Weapon Damage: " + gameManager.instance.playerWeaponManager.Damage + " + Bonus Damage: " + bonusDamage + " = " + finalDamage);
                    }
                }
            }

            if (gameManager.instance.playerWeaponManager.Ammo <= 0)
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

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        if (reloadTimer < gameManager.instance.playerWeaponManager.AmmoTimer)
        {
            gameManager.instance.playerWeaponManager.Ammo = 0;
            gameManager.instance.isReloading = true;
            StartReload();
        }
       
    }

    private void OnReloadCanceled(InputAction.CallbackContext context)
    {
        if (!gameManager.instance.isReloading)
        {
            gameManager.instance.Reload.SetActive(false);

        }
    }

    //this is now a Ability button instead of ADS

    private void OnADSPerformed(InputAction.CallbackContext context)
    {
       // switch (gameManager.instance.playerWeaponManager)


        gameManager.instance.isAiming = true;
        if (gameManager.instance.gameDebug)
        {
            Debug.Log("Aiming Down Sights!");
        }
    }

    //this is now a Ability button instead of ADS
    private void OnADSCanceled(InputAction.CallbackContext context)
    {
        gameManager.instance.isAiming = false;
        if (gameManager.instance.gameDebug)
        {
            Debug.Log("Stopped Aiming Down Sights!");
        }
    }



    private void ShootTimer()
    {
        if (gameManager.instance.isReloading)
        {            
            reloadTimer += Time.deltaTime;
            gameManager.instance.canShoot = false;

            if (reloadTimer >= gameManager.instance.playerWeaponManager.AmmoTimer)
            {
                gameManager.instance.playerWeaponManager.Ammo = gameManager.instance.playerWeaponManager.MaxAmmo;
                gameManager.instance.isReloading = false;
                isReloading = false;
                gameManager.instance.Reload.SetActive(false);
                reloadTimer = 0;
                gameManager.instance.canShoot = true;


                Debug.Log("Reload complete!");
            }
            
            return;
        }

        timer += Time.deltaTime;

        if (timer >= gameManager.instance.playerWeaponManager.Timer)
        {
            gameManager.instance.canShoot = true;
        }
    }


    private void HandleReload()
    {
        if (gameManager.instance.isReloading)
        {            
            gameManager.instance.reloadMax = gameManager.instance.playerWeaponManager.AmmoTimer;
            gameManager.instance.reloadTime = reloadTimer;

            if (isReloading)
            {
                gameManager.instance.Reload.SetActive(true);
                gameManager.instance.reloadBar.value = gameManager.instance.reloadTime / gameManager.instance.reloadMax;
            }
            else
            {
                gameManager.instance.Reload.SetActive(false);
            }
        }
        
    }
    private void StartReload()
    {
        
        if (gameManager.instance.playerWeaponManager.MaxAmmo <= 0)
        {
            return;
        }
        if (!isReloading)
        {
            gameManager.instance.Reload.SetActive(false);

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

        if (gameManager.instance.characterController.isGrounded)
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
}