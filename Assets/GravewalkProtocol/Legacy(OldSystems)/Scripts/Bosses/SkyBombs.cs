using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class SkyBombs : MonoBehaviour, IDamage
{
    [Header("Bomb Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] float gravity;
    
    [SerializeField] private GameObject explotion;
    [SerializeField] Rigidbody rb;

    [SerializeField] private BaseSoundSO _explode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth; 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Explode()
    {
        AudioManager.instance.PlaySound(_explode);
        explotion.SetActive(true);
        rb.isKinematic = true;
        yield return new WaitForSeconds(1f);
        explotion.SetActive(false);
        rb.isKinematic = false;
        gameObject.SetActive(false);
    }
    private void OnCollisionEnter(Collision collision)
    {
       StartCoroutine(Explode());        
    }

    void IDamage.takeDamage(int ammount)
    {
        currentHealth -= ammount;
    }
}
