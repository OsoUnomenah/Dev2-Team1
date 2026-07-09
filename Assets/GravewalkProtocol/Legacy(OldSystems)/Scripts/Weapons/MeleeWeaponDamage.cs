using System.Collections;
using UnityEngine;

public class MeleeWeaponDamage : MonoBehaviour
{
    [SerializeField] CapsuleCollider dmgTrigger;
    [SerializeField] Animator animator;

    private void Update()
    {
        
        if (gameManager.instance.canShoot == true)
        {
            dmgTrigger.enabled = false;
        }
        else
        {
            dmgTrigger.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {

            IDamage dmg = other.GetComponent<IDamage>();

            int bonusDamage = 0;

            StatHandler stats = gameManager.instance.playerStatHandler;

            if (stats != null)
            {
                bonusDamage = Mathf.RoundToInt(stats.modDamage);
            }

            dmg.takeDamage(gameManager.instance.playerWeaponManager.Damage + bonusDamage);
        }
    }
}
