using System;
using UnityEngine;
using System.Collections;

public class shooterEnemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Renderer model;

    [Header("Audio")]
    [SerializeField] private BaseSoundSO enemyShootSound;

    [Header("Stats")]
    [Range(1, 10)][SerializeField] int faceTargetSpeed;


    [Header("Weapon")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos1;
    [SerializeField] Transform shootPos2;
    [Range(0.1f, 2f)][SerializeField]float shootRate;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;


    
    Color originalColor;
    Vector3 playerDir;
    bool playerInTrigger;
    float shootTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
    private bool whichgun = false;
    private void shoot()
    {
            shootTimer = 0;
        if (whichgun)
        {
            Instantiate(bullet, shootPos1.position, gunPivot.rotation);
            whichgun = false;
        }
        else
        {
            Instantiate(bullet, shootPos2.position, gunPivot.rotation);
            whichgun= true;
        }
        PlayEnemyShootSound();
    }
    
     private void rotateGun()
    {
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
            model.material.color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
            model.material.color = originalColor;
    }

    private void PlayEnemyShootSound()
    {
        if (AudioManager.instance != null && enemyShootSound != null)
        {
            AudioManager.instance.PlaySoundAtPosition(enemyShootSound, gameObject);
        }
    }

   
}
