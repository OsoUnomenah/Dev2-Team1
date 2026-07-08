using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private LayerMask ignoreLayer;
    [SerializeField] private PlayerWeaponManager weaponManager;

    [Header("Player Settings")]
    [SerializeField] private int HP;
    [SerializeField] private int speed;
    [SerializeField] private int sprintMod;
    [SerializeField] private int jumpSpeed;
    [SerializeField] private int jumpMax;
    [SerializeField] private int gravity;

    private int jumpCount;
    private int HPOriginal;
    private float shootTimer;

    private Vector3 moveDir;
    private Vector3 playerVel;

    void Start()
    {
        HPOriginal = HP;

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (weaponManager == null)
        {
            weaponManager = GetComponent<PlayerWeaponManager>();
        }
    }

    void Update()
    {
        Movement();
        Sprint();
    }

    void Movement()
    {
        shootTimer += Time.deltaTime;

        if (controller != null && controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        if (controller != null)
        {
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        Jump();

        if (controller != null)
        {
            controller.Move(playerVel * Time.deltaTime);
        }

        playerVel.y -= gravity * Time.deltaTime;

        HandleShooting();
    }

    void HandleShooting()
    {
        if (weaponManager == null || !weaponManager.HasWeapon || Camera.main == null)
        {
            return;
        }

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * weaponManager.Range, Color.red);

        bool wantsToFire;

        if (weaponManager.CurrentWeaponData != null && weaponManager.CurrentWeaponData.fullAuto)
        {
            wantsToFire = Input.GetButton("Fire1");
        }
        else
        {
            wantsToFire = Input.GetButtonDown("Fire1");
        }

        Debug.Log(
    "Weapon: " + weaponManager.CurrentWeaponName +
    " | fullAuto: " + weaponManager.CurrentWeaponData.fullAuto +
    " | shootTimer: " + shootTimer +
    " | requiredTimer: " + weaponManager.Timer +
    " | ammo: " + weaponManager.Ammo
);
        if (wantsToFire && shootTimer >= weaponManager.Timer && weaponManager.Ammo > 0)
        {
            Shoot();
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void Shoot()
    {
        if (weaponManager == null || !weaponManager.HasWeapon || Camera.main == null)
        {
            return;
        }

        shootTimer = 0f;
        weaponManager.Ammo--;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, weaponManager.Range, ~ignoreLayer))
        {
            Debug.Log("Hitting: " + hit.collider.name);

            if (weaponManager.HitEffect != null)
            {
                Instantiate(weaponManager.HitEffect, hit.point, Quaternion.identity);
            }

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(weaponManager.Damage);
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }
}