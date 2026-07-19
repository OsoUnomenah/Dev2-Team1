using System.Collections;
using TMPro;
using UnityEngine;

public class turretTrap : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] Renderer model;
    public UnityEngine.UI.Slider healthbar;
    public TMP_Text healthText;
    public GameObject onScreenDMG;
    public TMP_Text damageText;
    [SerializeField] int xpGive = 100;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _hit;
    [SerializeField] BaseSoundSO _dead;
    [SerializeField] private BaseSoundSO turretShootSound;



    [Header("Stats")]
    [Range(10, 100)][SerializeField] int HP;
    [Range(10, 100)][SerializeField] int maxHP;


    [Range(1, 10)][SerializeField] int faceTargetSpeed;


    [Header("Weapon")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos1;
    [SerializeField] Transform shootPos2;
    [Range(0.1f, 2f)][SerializeField] float shootRate;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;
    [SerializeField] GameObject body;
    public ParticleSystem explosion;

    Color originalColor;
    Vector3 playerDir;
    bool playerInTrigger;
    float shootTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HP = maxHP;
       // gameManager.instance.updateGameGoal(1);
        originalColor = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        updateHealthBar();
        if (playerInTrigger)
        {
            shootTimer += Time.deltaTime;

            playerDir = gameManager.instance.player.transform.position - transform.position;
            faceTarget();
            rotateGun();
            if (shootTimer > shootRate)
            {
                shoot();
            }
        }
    }

    public void updateHealthBar()
    {
        healthText.text = HP + " / " + maxHP;
        healthbar.value = (float)HP / (float)maxHP;
    }

    IEnumerator updateDamageText()
    {
        damageText.text = ("DMG: " + gameManager.instance.playerDamageOut.ToString());

        onScreenDMG.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        onScreenDMG.SetActive(false);
    }

    private void faceTarget()
    {
        if (dead)
        {
            return;
        }
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);
    }
    private bool whichgun = false;
    private void shoot()
    {
        if (dead)
        {
            return;
        }
        shootTimer = 0;
        if (whichgun)
        {
            Instantiate(bullet, shootPos1.position, gunPivot.rotation);
            whichgun = false;
        }
        else
        {
            Instantiate(bullet, shootPos2.position, gunPivot.rotation);
            whichgun = true;
        }
        PlayTurretShootSound();
    }

    private void rotateGun()
    {
        if (dead)
        {
            return;
        }
        Quaternion rot;
        if (whichgun)
        {
            rot = Quaternion.LookRotation(playerDir);
            rot *= Quaternion.Euler(0f, -5f, 0f);
        }
        else
        {
            rot = Quaternion.LookRotation(playerDir);
            rot *= Quaternion.Euler(0f, 5f, 0f);
        }
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rot, faceTargetSpeed * Time.deltaTime * gunRotateSpeed);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            model.material.color = Color.mediumVioletRed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
            model.material.color = Color.green;

    }
    private bool dead = false;
    public void takeDamage(int amount)
    {
        gameManager.instance.playerDamageOut += amount;

        HP -= amount;
        //Show the damage text
        StartCoroutine(updateDamageText());

        if (HP <= 0)
        {
            dead = true;
            gameManager.instance.addXp(xpGive);
            //gameManager.instance.updateGameGoal(-1);
            StartCoroutine(death());
        }
        else
        {
            StartCoroutine(flashYellow());
        }
    }
    IEnumerator death()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }
    IEnumerator flashYellow()
    {
        model.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
    }

    private void PlayTurretShootSound()
    {
        if (AudioManager.instance != null && turretShootSound != null)
        {
            AudioManager.instance.PlaySoundAtPosition(turretShootSound, gameObject);
        }
    }
}
