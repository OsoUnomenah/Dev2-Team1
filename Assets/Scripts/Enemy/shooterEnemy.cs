using System;
using UnityEngine;
using System.Collections;

public class shooterEnemy : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] Renderer model;



    [Header("Stats")]
    [Range(10, 100)][SerializeField] int HP;
    [Range(1, 10)][SerializeField] int faceTargetSpeed;


    [Header("Weapon")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos;
    [Range(0.1f, 2f)][SerializeField]float shootRate;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;



    Color originalColor;
    Vector3 playerDir;
    bool playerInTrigger;
    float shootTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager.instance.updateGameGoal(1);
        originalColor = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {

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

   private void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0 , playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);
    }

    private void shoot()
    {
            shootTimer = 0;
            Instantiate(bullet, shootPos.position, gunPivot.rotation);
    }
    
     private void rotateGun()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rot, faceTargetSpeed * Time.deltaTime * gunRotateSpeed);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
    }
}
