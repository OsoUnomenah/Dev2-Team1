using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Player Settings")]
    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;

    [Header("Gun Settings")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootRange;
    [SerializeField] float speedRate;

    int jumpCount;
    int HPOriginal;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    bool isSprinting;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOriginal = HP;
        
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootRange, Color.red);

        if (Input.GetButton("Fire1") && shootTimer > speedRate)
        {
            shoot();
        }

    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void sprint()
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

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootRange, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }

    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP <= 0) 
        {
            gameManager.instance.youLose();
        }
    }
}
