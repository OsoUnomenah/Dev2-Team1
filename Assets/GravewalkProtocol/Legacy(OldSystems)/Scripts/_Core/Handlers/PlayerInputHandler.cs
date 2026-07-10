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
    [Range(10.0f, 80.0f)][SerializeField] private float acceleration = 10.0f;
    [Range(1.0f, 5.0f)][SerializeField] private float dashCd = 1.0f;
    [Range(1.0f, 30f)][SerializeField] float dashSpeed;
    [Range(1.0f, 200f)][SerializeField] float dashAttackSpeed;
    [Range(0.05f, 0.5f)][SerializeField] float dashTime;
    [Range(0, 30)][SerializeField] int dashFOVMod;

    [Header("Crouch Config")]
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float ceilingCheckRadius = 0.3f;
    [SerializeField] private LayerMask ceilingMask;

    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingCameraPosition;

    private bool isCrouching;
    private bool crouchRequested;

    private Vector3 dashVector;
    public Vector3 currentMovement;
    public float currentSpeed = 0f;
    private float dashTimer;

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

    [Header("Attacks Config")]
    [SerializeField] private float heavyAttackRadius = 4f;
    [SerializeField] private float aoeDelay = 0.5f;
    [SerializeField] private float dashAttackDelay = 0.5f;
    [SerializeField] public bool dashAttackTriggered;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _shoot;
    [SerializeField] BaseSoundSO _footsteps;
    [SerializeField] BaseSoundSO _dash;
    [SerializeField] private BaseSoundSO _dryFire;
    [Range(.4f, 1f)][SerializeField] private float footstepBaseInterval;
    [Range(.4f, 1f)][SerializeField] private float footstepSprintInterval = 0.5f;

    private float footstepTimer;
    private StatHandler playerStats;
    private bool isFrozenByBoss;
    private Coroutine freezeRoutine;


    // [Header("Combat Settings")] //Changed these to be exclusively tied to the WeaponManager values. 


    private PlayerActions playerActions; // Reference to the generated input actions class

    private InputAction moveAction;
    private InputAction rotateAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction crouchAction;
    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction adsAction;
    private InputAction pauseAction;

    [Header("GlobalVariables")]
    public bool DashTriggered { get; private set; }
    public Vector2 MovementVector { get; private set; }
    public Vector2 RotateVector { get; private set; }

    void Awake()
    {
        playerActions = new PlayerActions();

        moveAction = playerActions.PlayerInput.Movement;
        rotateAction = playerActions.PlayerInput.Rotate;

        jumpAction = playerActions.PlayerInput.Jump;
        dashAction = playerActions.PlayerInput.Sprint;
        crouchAction = playerActions.PlayerInput.Crouch;

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

        CharacterController controller = gameManager.instance.characterController;

        standingHeight = controller.height;
        standingCenter = controller.center;

        standingCameraPosition = gameManager.instance.playerCamera.transform.localPosition;
    }

    void Update()
    {

        if (isFrozenByBoss)
        {
            MovementVector = Vector2.zero;
            RotateVector = Vector2.zero;
            currentMovement = Vector3.zero;

            ShootTimer();
            HandleReload();
            return;
        }

        HandleCrouch();
        HandleMovement();
        HandleRotation();
        ApplyMovement();
        HandleDash();
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

        dashAction.performed += OnDashPerformed;
        dashAction.canceled += OnDashCanceled;

        crouchAction.performed += OnCrouchPerformed;
        crouchAction.canceled += OnCrouchCanceled;

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

        dashAction.performed -= OnDashPerformed;
        dashAction.canceled -= OnDashCanceled;

        crouchAction.performed -= OnCrouchPerformed;
        crouchAction.canceled -= OnCrouchCanceled;

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

        float targetSpeed = isCrouching
            ? crouchSpeed
            : walkSpeed;

        currentSpeed = Mathf.Lerp(
            currentSpeed,
            targetSpeed,
            Time.deltaTime * acceleration
        );

        currentMovement.x = worldDirection.x * currentSpeed;
        currentMovement.z = worldDirection.z * currentSpeed;
    }

    private void HandleCrouch()
    {
        if (crouchRequested)
        {
            isCrouching = true;
        }
        else if (isCrouching && CanStandUp())
        {
            isCrouching = false;
        }

        UpdateCrouchHeight();
        UpdateCrouchCamera();
    }

    private void UpdateCrouchHeight()
    {
        CharacterController controller =
            gameManager.instance.characterController;

        float targetHeight = isCrouching
            ? crouchHeight
            : standingHeight;

        Vector3 targetCenter = isCrouching
            ? GetCrouchingCenter()
            : standingCenter;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );

        controller.center = Vector3.Lerp(
            controller.center,
            targetCenter,
            Time.deltaTime * crouchTransitionSpeed
        );
    }

    private Vector3 GetCrouchingCenter()
    {
        float heightDifference = standingHeight - crouchHeight;

        return new Vector3(
            standingCenter.x,
            standingCenter.y - heightDifference * 0.5f,
            standingCenter.z
        );
    }

    private void UpdateCrouchCamera()
    {
        Transform playerCamera =
            gameManager.instance.playerCamera.transform;

        float heightDifference = standingHeight - crouchHeight;

        Vector3 crouchingCameraPosition =
            standingCameraPosition - new Vector3(
                0f,
                heightDifference * 0.5f,
                0f
            );

        Vector3 targetPosition = isCrouching
            ? crouchingCameraPosition
            : standingCameraPosition;

        playerCamera.localPosition = Vector3.Lerp(
            playerCamera.localPosition,
            targetPosition,
            Time.deltaTime * crouchTransitionSpeed
        );
    }

    private bool CanStandUp()
    {
        CharacterController controller =
            gameManager.instance.characterController;

        float radius = Mathf.Min(
            controller.radius * 0.9f,
            ceilingCheckRadius
        );

        Vector3 worldCenter =
            transform.TransformPoint(standingCenter);

        float halfHeight = Mathf.Max(
            standingHeight * 0.5f,
            radius
        );

        Vector3 bottomPoint =
            worldCenter + Vector3.down * (halfHeight - radius);

        Vector3 topPoint =
            worldCenter + Vector3.up * (halfHeight - radius);

        bool blocked = Physics.CheckCapsule(
            bottomPoint,
            topPoint,
            radius,
            ceilingMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (isFrozenByBoss)
        {
            return;
        }

        crouchRequested = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        crouchRequested = false;
    }

    private IEnumerator Dash()
    {
        float startTime = Time.time;
        dashTimer = 0f;

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            enemyLayer,
            true);

        gameManager.instance.playerCamera.fieldOfView += dashFOVMod;

        float speed;

        // move the character by some speed for a set time
        while (Time.time < startTime + dashTime)
        {
            if (dashAttackTriggered)
            {
                dashVector = gameManager.instance.playerCamera.transform.forward;
                speed = dashAttackSpeed;
            }
            else
            {
                dashVector = currentMovement;
                speed = dashSpeed;
            }

            gameManager.instance.characterController.Move(dashVector * speed * Time.deltaTime);
            yield return null;
        }

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            enemyLayer,
            false);

        gameManager.instance.playerCamera.fieldOfView -= dashFOVMod;
        gameManager.instance.isDashing = false;
    }

    private void HandleDash()
    {
        dashTimer += Time.deltaTime;

        if ((dashAction.WasPressedThisFrame() || dashAttackTriggered) && dashTimer > dashCd && gameManager.instance.canDash)
        {
            AudioManager.instance.PlaySoundFromSource(_dash, gameObject);
            gameManager.instance.isDashing = true;
            gameManager.instance.dashTriggered = true;
            StartCoroutine(Dash());
        }

        gameManager.instance.dashTriggered = false;
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(MovementVector.x, 0, MovementVector.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);

        return worldDirection.normalized;
    }

    private void ApplyMovement()
    {
        dashTimer += Time.deltaTime;

        gameManager.instance.characterController.Move(currentMovement * Time.deltaTime);
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        MovementVector = context.ReadValue<Vector2>();

        //if (turnOnDebug)
        //{
        //  Debug.Log(MovementVector);
        // }
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        MovementVector = Vector2.zero;

        // if (turnOnDebug)
        // {
        //  Debug.Log(MovementVector);
        // }
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        if (!gameManager.instance.isPaused)
        {
            RotateVector = context.ReadValue<Vector2>();

            //if (turnOnDebug)
            // {
            //Debug.Log(RotateVector);
            //}
        }
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        RotateVector = Vector2.zero;

        // if (turnOnDebug)
        // {
        //     Debug.Log(RotateVector);
        // }
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
            AudioManager.instance.PlaySoundFromSource(_footsteps, gameManager.instance.player);
            jumpCount++;
            JumpTriggered = false;
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    public void Bounce(int force)
    {
        currentMovement.y = force + (force * 5);
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

    public void OnDashPerformed(InputAction.CallbackContext obj)
    {

        // if (turnOnDebug)
        //{
        //     Debug.Log("Sprinting!");
        // }
    }

    private void OnDashCanceled(InputAction.CallbackContext context)
    {

        // if (turnOnDebug)
        //{
        //    Debug.Log("Sprinting Canceled!");
        //}
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isFrozenByBoss)
        {
            return;
        }


        // Debug.Log("InteractorSource: " + interactorSource);
        //Debug.Log("WeaponManager: " + gameManager.instance.playerWeaponManager);

        RaycastHit hit;
        if (Physics.Raycast(interactorSource.position, interactorSource.forward, out hit, interactRange, ~ignoreSource))
        {
            //  Debug.Log(hit.collider.name);

            IInteract iAct = hit.collider.GetComponentInParent<IInteract>();
            if (iAct != null)
            {
                iAct.Interact();
            }
        }

        Debug.DrawRay(interactorSource.position, interactorSource.forward * interactRange, Color.green);

        //if (gameManager.instance.gameDebug)
        // {
        //    Debug.Log("Interact Started!");
        //}
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        // if (gameManager.instance.gameDebug)
        // {
        //     Debug.Log("Stopped Interacting!");
        // }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {

        if (isFrozenByBoss)
        {
            return;
        }



        if (gameManager.instance.playerWeaponManager == null)
        {
            //Debug.Log("Can't find the weapon manager");
            return;
        }

        // No weapon equipped / invalid weapon = no sound.
        if (gameManager.instance.playerWeaponManager.Damage <= 0
            || gameManager.instance.playerWeaponManager.Range <= 0)
        {
            // Debug.Log("Can't shoot, Range or damage is 0");
            return;
        }

        if (gameManager.instance.isReloading)
        {
            // Debug.Log("Cannot shoot while reloading.");
            return;
        }

        // Weapon is equipped, but ammo is already empty = dry fire.
        if (gameManager.instance.playerWeaponManager.Ammo <= 0 && gameManager.instance.playerWeaponManager.Type == false)
        {
            PlayDryFireSound();
            // Debug.Log("Out of ammo. Press reload.");
            return;
        }

        if (gameManager.instance.canShoot == true)
        {
            recoil = gameManager.instance.recoil;
        }
        else
        {
            recoil = 0;
        }


        // Debug.Log("Shoot interaction" + context.interaction);

        if (!gameManager.instance.isPaused && !gameManager.instance.isLevelingUp && gameManager.instance.canShoot == true)
        {
            timer = 0;
            gameManager.instance.canShoot = false;

            
            if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
            {
                if (gameManager.instance.playerWeaponManager.Type == true && gameManager.instance.canMelee)
                {
                    
                    gameManager.instance.isMeleeing = true;
                    gameManager.instance.playerWeaponManager.PlayMeleeHeavyAttack();

                    if (gameManager.instance.playerWeaponManager.CurrentWeaponName == "Hammer")
                    {
                        Debug.Log("Hammer special");
                        StartCoroutine(HeavyAttackAOE());
                    }
                    else if (gameManager.instance.playerWeaponManager.CurrentWeaponName == "Katana")
                    {
                        gameManager.instance.playerWeaponManager.Timer = 2.0f;
                        StartCoroutine(KatanaDashAttack());
   
                    }
                }
            }
            else if (context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
            {
                if (gameManager.instance.playerWeaponManager.Type == false)
                {
                    PlayCurrentWeaponShootSound();
                    gameManager.instance.playerWeaponManager.Ammo--;

                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, gameManager.instance.playerWeaponManager.Range, ~ignoreSource))
                    {
                        Debug.Log(hit.collider.name);

                        if (gameManager.instance.playerWeaponManager.HitEffect != null)
                        {
                            // Spawns the effect exactly where the raycast hit, facing away from the surface
                            Instantiate(
                                gameManager.instance.playerWeaponManager.HitEffect,
                                hit.point,
                                Quaternion.LookRotation(hit.normal)
                            );
                        }

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

                            // if (turnOnDebug)
                            //  {
                            //     Debug.Log("Weapon Damage: " + gameManager.instance.playerWeaponManager.Damage + " + Bonus Damage: " + bonusDamage + " = " + finalDamage);
                            // }
                        }
                    }
                }
                else if(gameManager.instance.canMelee)
                {
                    PlayCurrentWeaponShootSound();
                    gameManager.instance.isMeleeing = true;

                    gameManager.instance.playerWeaponManager.PlayMeleeLightAttack();
                }

                //if (turnOnDebug)
                //{
                //     Debug.Log("ShotFired!");
                //}

            }
        }

    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        // cancel logic for button release if needed
    }


    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        if (isFrozenByBoss)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(interactorSource.position, interactorSource.forward, out hit, interactRange, ~ignoreSource))
        {
            IInteract iAct = hit.collider.GetComponentInParent<IInteract>();
            if (iAct != null)
            {
                iAct.Interact();
            }
        }


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
        
    }
    IEnumerator fireCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        gameManager.instance.allowedAbility1 = true;
    }
    IEnumerator freezeCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        gameManager.instance.allowedAbility2 = true;
    }
    public IEnumerator bounceCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        gameManager.instance.allowedAbility3 = true;
    }
    IEnumerator zoomCooldown(float cd)
    {
        yield return new WaitForSeconds(1.5f);
        gameManager.instance.allowedAbility4 = true;
    }
  
    

    //this is now a Ability button instead of ADS
    private void OnADSCanceled(InputAction.CallbackContext context)
    {
        gameManager.instance.isAiming = false;
        //if (gameManager.instance.gameDebug)
        //{
        //     Debug.Log("Stopped Aiming Down Sights!");
        //}
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


                // Debug.Log("Reload complete!");
            }

            return;
        }

        timer += Time.deltaTime;

        if (timer >= gameManager.instance.playerWeaponManager.Timer)
        {
            gameManager.instance.canShoot = true;
            gameManager.instance.playerWeaponManager.ResetMeleeAnimationTriggers();
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

        if (isReloading)
        {
            return;
        }

        isReloading = true;
        gameManager.instance.isReloading = true;
        reloadTimer = 0;
        gameManager.instance.canShoot = false;



        if (gameManager.instance.Reload != null)
        {
            gameManager.instance.Reload.SetActive(true);
        }

        PlayCurrentWeaponReloadSound();

        //Debug.Log("Reloading...");
    }

    private void PlayCurrentWeaponShootSound()
    {
        BaseSoundSO soundToPlay = gameManager.instance.playerWeaponManager.ShootSound;

        if (gameManager.instance.playerWeaponManager != null &&
            gameManager.instance.playerWeaponManager.ShootSound != null)
        {
            soundToPlay = gameManager.instance.playerWeaponManager.ShootSound;
        }

        if (AudioManager.instance != null && soundToPlay != null)
        {
            AudioManager.instance.PlaySound(soundToPlay);
        }
    }

    private void PlayCurrentWeaponReloadSound()
    {
        if (gameManager.instance.playerWeaponManager == null)
        {
            return;
        }

        BaseSoundSO soundToPlay = gameManager.instance.playerWeaponManager.ReloadSound;

        if (AudioManager.instance != null && soundToPlay != null)
        {
            AudioManager.instance.PlaySound(soundToPlay);
        }
    }

    private void PlayDryFireSound()
    {
        if (AudioManager.instance != null && _dryFire != null)
        {
            AudioManager.instance.PlaySound(_dryFire);
        }
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

            float interval = gameManager.instance.isDashing
                ? footstepBaseInterval * footstepSprintInterval
                : footstepBaseInterval;

            if (footstepTimer >= interval)
            {
                AudioManager.instance.PlaySoundFromSource(_footsteps, gameManager.instance.player);
                footstepTimer = 0;
            }
        }
    }

    IEnumerator HeavyAttackAOE()
    {
        // Debug.Log("Heavy attack aoe");
        yield return new WaitForSeconds(aoeDelay);

        PlayCurrentWeaponShootSound();

        Collider[] hits = Physics.OverlapSphere(gameManager.instance.player.transform.position, heavyAttackRadius, enemyLayer);

        foreach (Collider others in hits)
        {
            IDamage dmg = others.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(gameManager.instance.playerWeaponManager.Damage);
            }
        }
    }

    IEnumerator KatanaDashAttack()
    {

     
        yield return new WaitForSeconds(dashAttackDelay);

        PlayCurrentWeaponShootSound();

        dashAttackTriggered = true;

        yield return new WaitForSeconds(0.5f);

        dashAttackTriggered = false;
        gameManager.instance.playerWeaponManager.Timer = gameManager.instance.playerWeaponManager.TimerOrig;
    }

    public void FreezePlayer(float duration)
    {
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        freezeRoutine = StartCoroutine(FreezePlayerRoutine(duration));
    }

    private IEnumerator FreezePlayerRoutine(float duration)
    {
        isFrozenByBoss = true;

        yield return new WaitForSeconds(duration);

        isFrozenByBoss = false;
    }
}