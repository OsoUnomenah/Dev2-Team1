using System.Collections;
using UnityEngine;

public class MeleeWeaponDamage : MonoBehaviour
{
    [SerializeField] CapsuleCollider dmgTrigger;
    [SerializeField] Animator animator;

    private void Update()
    {
        
        if (gameManager.instance.isMeleeing == true)
        {
            dmgTrigger.enabled = true;
        }
        else
        {
            dmgTrigger.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {

            IDamage dmg = other.GetComponent<IDamage>();

            int bonusDamage = 0;

            if (gameManager.instance.playerInputHandler.dashAttackTriggered)
            {
                bonusDamage += 85;
            }

            StatHandler stats = gameManager.instance.playerStatHandler;

            if (stats != null)
            {
                bonusDamage += Mathf.RoundToInt(stats.modDamage);
            }

            dmg.takeDamage(gameManager.instance.playerWeaponManager.Damage + bonusDamage);
        }
    }
}
