using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

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
    [Range(0.5f, 5.0f)][SerializeField] private float dashCd = 1.0f;
    [Range(1.0f, 30f)][SerializeField] float dashSpeed;
    [Range(1.0f, 200f)][SerializeField] float dashAttackSpeed;
    [Range(0.05f, 0.5f)][SerializeField] float dashTime;
    [Range(0, 30)][SerializeField] int dashFOVMod;

    [Header("Freeze Weapon Effect")]
    [SerializeField] private float gunFreezeChance = 0.25f;
    [SerializeField] private float gunFreezeCooldown = 0.15f;
    [SerializeField] private float meleeFreezeCooldown = 3f;
    [SerializeField] private float defaultFreezeDuration = 2f;

    private float nextGunFreezeTime;
    private float nextMeleeFreezeTime;

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
    [SerializeField] public LayerMask enemyLayer;

    [Header("Charged Shot Runtime")]
    [SerializeField] private bool isChargingShot;
    [SerializeField] private float currentChargeTime;
    [SerializeField] private float chargedShotCooldownTimer;

    private float normalCameraFOV;

    [Header("Charged Shot UI")]
    [SerializeField] private GameObject sniperChargePanel;
    [SerializeField] private Slider sniperChargeSlider;
    [SerializeField] private TMP_Text sniperChargeText;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _shoot;
    [SerializeField] BaseSoundSO _footsteps;
    [SerializeField] BaseSoundSO _dash;
    [SerializeField] BaseSoundSO jumpSound;
    [SerializeField] BaseSoundSO landSound;
    [SerializeField] BaseSoundSO crouchSound;
    [SerializeField] BaseSoundSO standSound;
    [SerializeField] private BaseSoundSO _dryFire;
    [Range(.4f, 1f)][SerializeField] private float footstepBaseInterval;
    [Range(.4f, 1f)][SerializeField] private float footstepSprintInterval = 0.5f;

    private float footstepTimer;
    private bool wasGrounded;
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
        fov = gameManager.instance.playerCamera.fieldOfView;
        normalCameraFOV =
        gameManager.instance.playerCamera.fieldOfView;

        sniperChargePanel = gameManager.instance.sniperChargePanel;
        sniperChargeSlider = gameManager.instance.sniperChargeSlider;
        sniperChargeText = gameManager.instance.sniperChargeText;
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
            HandleChargedShot();
            UpdateChargedShotUI();
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
        HandleChargedShot();
        UpdateChargedShotUI();
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

        shootAction.started += OnShootStarted;
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

        shootAction.started -= OnShootStarted;
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

        if (!isCrouching && AudioManager.instance != null && crouchSound != null)
        {
            AudioManager.instance.PlaySoundFromSource(
                crouchSound,
                gameManager.instance.player
            );
        }
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        crouchRequested = false;

        if (isCrouching && CanStandUp() &&
    AudioManager.instance != null && standSound != null)
        {
            AudioManager.instance.PlaySoundFromSource(
                standSound,
                gameManager.instance.player
            );
        }
    }

    private IEnumerator Dash()
    {
        float startTime = Time.time;
        dashTimer = 0f;

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        Physics.IgnoreLayerCollision(playerLayer,enemyLayer,true);

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

        Physics.IgnoreLayerCollision(playerLayer,enemyLayer,false);

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

            if (AudioManager.instance != null && jumpSound != null)
            {
                AudioManager.instance.PlaySoundFromSource(
                    jumpSound,
                    gameManager.instance.player
                );
            }

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

    private void OnShootStarted(InputAction.CallbackContext context)
    {
        if (isFrozenByBoss)
        {
            return;
        }

        if (gameManager.instance.isPaused ||
            gameManager.instance.isLevelingUp)
        {
            return;
        }

        PlayerWeaponManager weaponManager =
            gameManager.instance.playerWeaponManager;

        if (weaponManager == null)
        {
            return;
        }

        if (!weaponManager.UsesChargedShot)
        {
            return;
        }

        if (chargedShotCooldownTimer > 0f)
        {
            return;
        }

        BeginChargedShot();
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

        PlayerWeaponManager weaponManager = gameManager.instance.playerWeaponManager;

        if (weaponManager.UsesChargedShot)
        {
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

                    // One trigger pull consumes one shell, regardless of pellet count.
                    gameManager.instance.playerWeaponManager.Ammo--;
                    _ =
                        gameManager.instance.playerWeaponManager;

                    int pelletCount = weaponManager.UsesPellets
                        ? Mathf.Max(1, weaponManager.PelletCount)
                        : 1;
                    Debug.Log(
                            "SHOTGUN DEBUG | Weapon: " + weaponManager.CurrentWeaponName +
                            " | UsesPellets: " + weaponManager.UsesPellets +
                            " | PelletCount: " + pelletCount +
                            " | Horizontal: " + weaponManager.HorizontalSpread +
                            " | Vertical: " + weaponManager.VerticalSpread
                            );

                    for (int pelletIndex = 0; pelletIndex < pelletCount; pelletIndex++)
                    {

                        Vector3 shotDirection = GetPelletDirection(
                                pelletIndex,
                                pelletCount,
                                weaponManager.HorizontalSpread,
                                weaponManager.VerticalSpread,
                                weaponManager.UsesPellets
                                );

                        RaycastHit hit;

                        if (Physics.Raycast(
                            Camera.main.transform.position,
                            shotDirection,
                            out hit,
                            weaponManager.Range,
                            ~ignoreSource))
                        {

                            if (weaponManager.HitEffect != null)
                            {
                                Instantiate(
                                    weaponManager.HitEffect,
                                    hit.point,
                                    Quaternion.LookRotation(hit.normal)
                                );
                            }

                            IDamage dmg = hit.collider.GetComponentInChildren<IDamage>();
                            if (gameManager.instance.playerWeaponManager.abilities.Count > 0)
                            {
                                switch (gameManager.instance.playerWeaponManager.abilities[gameManager.instance.playerWeaponManager.abilitySlot].abilityType)
                                {
                                    //Examples for IDamage are right below here, you may need to make your bullets deal damage too,
                                    //infact most should still run the IDamage thing below,
                                    //but might have to change damage values or something in here first
                                    //Also, melee weapons call their stuff in their own methods, so you'll need to go into them and just make sure they're working
                                    //personally I'll 
                                    case AbilityStats.ability.fire:
                                        //probably use a IFire interface that works like IDamage but makes them set fire
                                        break;
                                    case AbilityStats.ability.freeze:
                                        TryApplyWeaponFreeze(hit.collider, false);
                                        break;
                                    case AbilityStats.ability.toxic:
                                        //probably use a IToxic interface that works like IDamage but makes them become toxic
                                        break;
                                    case AbilityStats.ability.magent:
                                        //good luck lol idk
                                        break;
                                    case AbilityStats.ability.crystal:
                                        CrystalShot(dmg, hit);
                                        break;
                                    case AbilityStats.ability.lightning:
                                        //chain lightning, probably also use a ILightning interface but may have to rework the enemies a bit to be able to actually chain the lightning together
                                        break;

                                }
                            }

                            if (dmg != null && gameManager.instance.playerWeaponManager.Damage != 0)
                            {
                                int bonusDamage = 0;

                                StatHandler stats = gameManager.instance.playerStatHandler;

                                if (stats != null)
                                {
                                    bonusDamage = Mathf.RoundToInt(stats.modDamage);
                                }


                                int finalDamage = gameManager.instance.playerWeaponManager.Damage + bonusDamage;
                                if (gameManager.instance.playerStatHandler.crystalBar == 10)
                                {
                                    gameManager.instance.playerStatHandler.crystalBar = 0;
                                    finalDamage *= 3;
                                }

                                dmg.takeDamage(finalDamage);

                                // if (turnOnDebug)
                                //  {
                                //     Debug.Log("Weapon Damage: " + gameManager.instance.playerWeaponManager.Damage + " + Bonus Damage: " + bonusDamage + " = " + finalDamage);
                                // }
                            }
                        }
                    }
                }
                else if (gameManager.instance.canMelee)
                {
                    PlayCurrentWeaponShootSound();
                    gameManager.instance.isMeleeing = true;

                    gameManager.instance.playerWeaponManager.PlayMeleeLightAttack();
                    StartCoroutine(LightAttack());

                }

                    //if (turnOnDebug)
                    //{
                    //     Debug.Log("ShotFired!");
                    //}





                }
            }
         }

    private Vector3 GetPelletDirection(
    int pelletIndex,
    int pelletCount,
    float horizontalSpread,
    float verticalSpread,
    bool useSpread)
    {
        Transform cameraTransform = Camera.main.transform;

        if (!useSpread || pelletCount <= 1)
        {
            return cameraTransform.forward;
        }

        float horizontalPosition =
            (float)pelletIndex / (pelletCount - 1);

        horizontalPosition =
            horizontalPosition * 2f - 1f;

        float horizontalOffset =
            horizontalPosition * horizontalSpread;

        float verticalOffset =
            UnityEngine.Random.Range(
                -verticalSpread,
                verticalSpread
            );

        Quaternion spreadRotation = Quaternion.Euler(
            verticalOffset,
            horizontalOffset,
            0f
        );

        Vector3 localDirection =
            spreadRotation * Vector3.forward;

        return cameraTransform
            .TransformDirection(localDirection)
            .normalized;
    }
    
    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        // cancel logic for button release if needed

       

        if (gameManager.instance.playerWeaponManager == null)
        {
            return;
        }

        if (!gameManager.instance.playerWeaponManager.UsesChargedShot)
        {
            return;
        }

        if (!isChargingShot)
        {
            return;
        }

        FireChargedShot();
    }

    private void FireChargedShot()
    {
        PlayerWeaponManager weaponManager =
            gameManager.instance.playerWeaponManager;

        float chargePercent = GetChargePercent();

        float chargeMultiplier = Mathf.Lerp(
            1f,
            weaponManager.MaxChargeMultiplier,
            chargePercent
        );

        int bonusDamage = 0;

        StatHandler stats =
            gameManager.instance.playerStatHandler;

        if (stats != null)
        {
            bonusDamage = Mathf.RoundToInt(stats.modDamage);
        }

        int finalDamage = Mathf.RoundToInt(
            (weaponManager.Damage + bonusDamage) *
            chargeMultiplier
        );

        bool fullyCharged = chargePercent >= 0.99f;

        if (fullyCharged)
        {
            finalDamage = Mathf.RoundToInt(
                finalDamage *
                weaponManager.CriticalMultiplier
            );

            Debug.Log("FULL CHARGE CRITICAL: " + finalDamage);
        }

        PlayCurrentWeaponShootSound();

        RaycastHit hit;

        if (Physics.Raycast(
            Camera.main.transform.position,
            Camera.main.transform.forward,
            out hit,
            weaponManager.Range,
            ~ignoreSource))
        {
            if (weaponManager.HitEffect != null)
            {
                Instantiate(
                    weaponManager.HitEffect,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
            }

            TryApplyWeaponFreeze(hit.collider, false);

            IDamage damageTarget =
                hit.collider.GetComponentInChildren<IDamage>();

            if (damageTarget == null)
            {
                damageTarget =
                    hit.collider.GetComponentInParent<IDamage>();
            }

            if (damageTarget != null)
            {
                damageTarget.takeDamage(finalDamage);
            }
        }

        EndChargedShot();
    }

    private void EndChargedShot()
    {
        gameManager.instance.canShoot = false;
        

        isChargingShot = false;
        currentChargeTime = 0f;

        chargedShotCooldownTimer = gameManager.instance.playerWeaponManager.ChargeCooldown;
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
    float range;
    float adsRecoil;
    float fov;
    private Coroutine adsInCoroutine;
    private Coroutine adsOutCoroutine;
    private void OnADSPerformed(InputAction.CallbackContext context)
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
        range = gameManager.instance.playerWeaponManager.Range;
        adsRecoil = gameManager.instance.playerWeaponManager.Recoil;
        

        if (!gameManager.instance.isAiming && !gameManager.instance.playerWeaponManager.Type)
        {
            if(adsOutCoroutine != null)
            {
                StopCoroutine(adsOutCoroutine);
            }
            gameManager.instance.playerWeaponManager.Range = range * 1.4f;
            gameManager.instance.playerWeaponManager.Recoil = adsRecoil - 0.2f;
            adsInCoroutine = StartCoroutine(AdsIn());
            gameManager.instance.isAiming = true;
        }

    }
    IEnumerator AdsIn()
    {
        float timer = 0f;
        float duration = 0.3f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            gameManager.instance.playerCamera.fieldOfView = Mathf.Lerp(
                     gameManager.instance.playerCamera.fieldOfView,
                     fov - 30,
                     timer / duration);

            gameManager.instance.playerWeaponManager.weaponHolder.transform.position = Vector3.Lerp(
                gameManager.instance.playerWeaponManager.weaponHolder.transform.position,
                gameManager.instance.playerWeaponManager.adsWeaponHolder.transform.position,
                timer / duration);

            yield return null;
        }
        gameManager.instance.playerCamera.fieldOfView = fov - 30;
        gameManager.instance.playerWeaponManager.weaponHolder.transform.position = gameManager.instance.playerWeaponManager.adsWeaponHolder.transform.position;
        
    }
    private void OnADSCanceled(InputAction.CallbackContext context)
    {
        if (gameManager.instance.isAiming && !gameManager.instance.playerWeaponManager.Type)
        {
            if (adsInCoroutine != null)
            {
                StopCoroutine(adsInCoroutine);
            }
            gameManager.instance.playerWeaponManager.Range = range;
            gameManager.instance.playerWeaponManager.Recoil = adsRecoil;
            adsOutCoroutine = StartCoroutine(AdsOut());

            gameManager.instance.isAiming = false;
        }
    }
    IEnumerator AdsOut()
    {
        float timer = 0f;
        float duration = 0.3f;
        while (timer < duration)
        {
            
            timer += Time.deltaTime;
            gameManager.instance.playerCamera.fieldOfView = Mathf.Lerp(
                     gameManager.instance.playerCamera.fieldOfView,
                     fov,
                     timer / duration);

            gameManager.instance.playerWeaponManager.weaponHolder.transform.position = Vector3.Lerp(
                gameManager.instance.playerWeaponManager.weaponHolder.transform.position,
                gameManager.instance.playerWeaponManager.nonADSWeaponHolder.transform.position,
                timer / duration);

            yield return null;
        }
        gameManager.instance.playerCamera.fieldOfView = fov;
        gameManager.instance.playerWeaponManager.weaponHolder.transform.position = gameManager.instance.playerWeaponManager.nonADSWeaponHolder.transform.position;

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
        yield return new WaitForSeconds(cd);
        gameManager.instance.allowedAbility4 = true;
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

    private void HandleLandingSound()
    {
        bool isGrounded = gameManager.instance.characterController.isGrounded;

        if (!wasGrounded && isGrounded)
        {
            if (AudioManager.instance != null && landSound != null)
            {
                AudioManager.instance.PlaySoundFromSource(landSound, gameManager.instance.player);
            }
        }
        wasGrounded = isGrounded;
    }

    IEnumerator LightAttack()
    {
        // Debug.Log("Heavy attack aoe");
        yield return new WaitForSeconds(aoeDelay);

        PlayCurrentWeaponShootSound();

        Collider[] hits = Physics.OverlapSphere(gameManager.instance.player.transform.position, heavyAttackRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider others in hits)
        {
            IDamage dmg = others.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(gameManager.instance.playerWeaponManager.Damage);
            }
        }
    }

    IEnumerator HeavyAttackAOE()
    {
        // Debug.Log("Heavy attack aoe");
        yield return new WaitForSeconds(aoeDelay);

        PlayCurrentWeaponShootSound();

        Collider[] hits = Physics.OverlapSphere(gameManager.instance.player.transform.position, heavyAttackRadius, LayerMask.GetMask("Enemy"));

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
        Collider[] hits = Physics.OverlapSphere(gameManager.instance.player.transform.position, heavyAttackRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider others in hits)
        {
            IDamage dmg = others.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(gameManager.instance.playerWeaponManager.Damage);
            }
        }

        dashAttackTriggered = false;
        gameManager.instance.playerWeaponManager.Timer = gameManager.instance.playerWeaponManager.TimerOrig;
    }

    private bool CurrentAbilityIsFreeze(out AbilityStats freezeStats)
    {
        freezeStats = null;

        PlayerWeaponManager weaponManager = gameManager.instance.playerWeaponManager;

        if (weaponManager == null || weaponManager.abilities == null)
        {
            return false;
        }

        if (weaponManager.abilitySlot < 0 || weaponManager.abilitySlot >= weaponManager.abilities.Count)
        {
            return false;
        }

        freezeStats = weaponManager.abilities[weaponManager.abilitySlot];

        if (freezeStats == null)
        {
            return false;
        }

        return freezeStats.abilityType == AbilityStats.ability.freeze;
    }

    private IFreeze FindFreezeTarget(Collider hitCollider)
    {
        IFreeze freezeTarget = hitCollider.GetComponent<IFreeze>();

        if (freezeTarget != null)
        {
            return freezeTarget;
        }

        freezeTarget = hitCollider.GetComponentInParent<IFreeze>();

        if (freezeTarget != null)
        {
            return freezeTarget;
        }

        return hitCollider.GetComponentInChildren<IFreeze>();
    }

    private IShatterable FindShatterTarget(Collider hitCollider)
    {
        IShatterable shatterable = hitCollider.GetComponent<IShatterable>();

        if (shatterable != null)
        {
            return shatterable;
        }

        shatterable = hitCollider.GetComponentInParent<IShatterable>();

        if (shatterable != null)
        {
            return shatterable;
        }

        return hitCollider.GetComponentInChildren<IShatterable>();
    }
    private void CrystalShot(IDamage dmg, RaycastHit hit)
    {
        if (dmg != null)
        {
            gameManager.instance.playerStatHandler.crystalBar += 1;
        }
    }
    public void TryApplyWeaponCrystal(Collider hitCollider, ref int multiplier)
    {
        if (gameManager.instance.playerWeaponManager.abilities.Count == 0)
        {
            return;
        }
        if (gameManager.instance.playerWeaponManager.abilities[gameManager.instance.playerWeaponManager.abilitySlot].abilityType == AbilityStats.ability.crystal
            && hitCollider.GetComponent<IDamage>() != null)
        {
            gameManager.instance.playerStatHandler.crystalBar += 1;
            if (gameManager.instance.playerStatHandler.crystalBar == 10)
            {
                multiplier = 3;
                gameManager.instance.playerStatHandler.crystalBar = 0;
                return;
            }
        }
        multiplier = 1;
    }
    public void TryApplyWeaponFreeze(Collider hitCollider, bool isMelee)
    {
        if (!CurrentAbilityIsFreeze(out AbilityStats freezeStats))
        {
            return;
        }

        float freezeDuration = freezeStats.effectTimer > 0
            ? freezeStats.effectTimer
            : defaultFreezeDuration;

        if (isMelee)
        {
            if (Time.time < nextMeleeFreezeTime)
            {
                return;
            }

            nextMeleeFreezeTime = Time.time + meleeFreezeCooldown;
        }
        else
        {
            if (Time.time < nextGunFreezeTime)
            {
                return;
            }

            if (UnityEngine.Random.value > gunFreezeChance)
            {
                return;
            }

            nextGunFreezeTime = Time.time + gunFreezeCooldown;
        }

        IFreeze freezeTarget = FindFreezeTarget(hitCollider);

        if (freezeTarget != null)
        {
            freezeTarget.freeze(freezeDuration);
        }
    }

    public bool TryShatterFrozenTarget(Collider hitCollider)
    {
        IShatterable shatterable = FindShatterTarget(hitCollider);

        if (shatterable != null && shatterable.IsFrozen)
        {
            shatterable.Shatter();
            return true;
        }

        return false;
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

    private void HandleChargedShot()
    {
        PlayerWeaponManager weaponManager =
            gameManager.instance.playerWeaponManager;

        if (weaponManager == null)
        {
            return;
        }

        if (chargedShotCooldownTimer > 0f)
        {
            chargedShotCooldownTimer -= Time.deltaTime;
        }

        if (!isChargingShot)
        {
            return;
        }

        currentChargeTime += Time.deltaTime;

        currentChargeTime = Mathf.Clamp(
            currentChargeTime,
            0f,
            weaponManager.ChargeTime
        );

        //float chargePercent = GetChargePercent();

       // gameManager.instance.playerCamera.fieldOfView =
           // Mathf.Lerp(
           //     normalCameraFOV,
            //    weaponManager.ChargedZoomFOV,
            //    chargePercent
           // );
    }

    private float GetChargePercent()
    {
        PlayerWeaponManager weaponManager =
            gameManager.instance.playerWeaponManager;

        if (weaponManager == null || weaponManager.ChargeTime <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(
            currentChargeTime / weaponManager.ChargeTime
        );
    }

    private void BeginChargedShot()
    {
        if (chargedShotCooldownTimer > 0f)
        {
            return;
        }

        if (isChargingShot)
        {
            return;
        }

        isChargingShot = true;
        currentChargeTime = 0f;
    }

    private void UpdateChargedShotUI()
    {
        PlayerWeaponManager weaponManager = gameManager.instance.playerWeaponManager;

        if (weaponManager == null)
            return;

        // Only show this UI for charged weapons
        if (!weaponManager.UsesChargedShot)
        {
            sniperChargePanel.SetActive(false);
            return;
        }

        // Show panel while charging or recharging
        if (isChargingShot)
        {
            sniperChargePanel.SetActive(true);
        }
        else
        {
            sniperChargePanel.SetActive(false);
            return;
        }

        if (isChargingShot)
        {
            float chargePercent = currentChargeTime / weaponManager.ChargeTime;
            chargePercent = Mathf.Clamp01(chargePercent);

            sniperChargeSlider.value = chargePercent;

            if (chargePercent >= 1f)
            {
                sniperChargeText.text = "CRITICAL READY";
            }
            else
            {
                sniperChargeText.text = "CHARGING";
            }
        }
        else
        {
            float rechargePercent =
                1f - (chargedShotCooldownTimer / weaponManager.ChargeCooldown);

            rechargePercent = Mathf.Clamp01(rechargePercent);

            sniperChargeSlider.value = rechargePercent;
            sniperChargeText.text = "RECHARGING";
        }
    }
}