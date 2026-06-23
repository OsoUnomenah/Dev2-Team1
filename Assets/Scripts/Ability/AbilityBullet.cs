using System.Collections;
using UnityEngine;

public class abilityBullet : MonoBehaviour
{
    enum damageType { fire, freeze, bounce, zoom }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDestroyTime;
    [SerializeField] ParticleSystem hitEffect;


    bool isDamaging;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.linearVelocity = transform.forward * bulletSpeed;
        Destroy(gameObject, bulletDestroyTime);

    }

    private void Update()
    {
       
    }
    

    private void OnTriggerEnter(Collider other)
    {
        

        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type == damageType.fire)
        {
            ParticleSystem effect = null;

            if (hitEffect != null)
            {
               effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
               effect.Play();
            }
            
            StartCoroutine(fireDamage(dmg, other));
           
            Destroy(effect);
            return;
        }

        IFreeze frz = other.GetComponent<IFreeze>();
        if (frz != null && type == damageType.freeze)
        {
            ParticleSystem effect = null;

            if (hitEffect != null)
            {
                effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                effect.Play();
            }

            frz.freeze(gameManager.instance.playerWeaponManager
                        .abilities[gameManager.instance.playerWeaponManager.abilitySlot]
                        .effectTimer);
            Destroy(effect);
            
            
            
            Destroy(gameObject);
        }
    }
    IEnumerator fireDamage(IDamage d, Collider other)
    {
        isDamaging = true;
        float duration = gameManager.instance.playerWeaponManager
        .abilities[gameManager.instance.firePos]
        .effectTimer;

        float endTime = Time.time + duration;

        enemyAI enemy = other.GetComponent<enemyAI>();        

        while (Time.time < endTime)
        {
            if (enemy != null && enemy.isDead)
                yield break;

            d.takeDamage(damageAmount + gameManager.instance.playerWeaponManager.fireLevel * (int)gameManager.instance.playerStatHandler.modDamage);
            yield return new WaitForSeconds(2f);
        }
        isDamaging = false;
        
        Destroy(gameObject);
        
    }

   
}
