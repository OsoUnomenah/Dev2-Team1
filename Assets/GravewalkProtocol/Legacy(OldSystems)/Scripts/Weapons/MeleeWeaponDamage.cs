using System.Collections;
using UnityEngine;

public class MeleeWeaponDamage : MonoBehaviour
{
    [SerializeField] CapsuleCollider dmgTrigger;
    [SerializeField] Animator animator;

    float timer = 0f;
    float waitTime;

    private void Update()
    {
        timer += Time.deltaTime;

        if(gameManager.instance == null || gameManager.instance.playerWeaponManager == null)
        {
            return;
        }

        waitTime = gameManager.instance.playerWeaponManager.Timer;

        if (gameManager.instance.isMeleeing == true && timer > waitTime)
        {
            dmgTrigger.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        timer = 0;
        if (other.CompareTag("Enemy"))
        {
            dmgTrigger.enabled = false;
            Debug.Log("Attempted to deal damage");

            if (gameManager.instance.playerInputHandler.TryShatterFrozenTarget(other))
            {
                return;
            }
            int multiplier = 0;
            gameManager.instance.playerInputHandler.TryApplyWeaponFreeze(other, true);
            gameManager.instance.playerInputHandler.TryApplyWeaponCrystal(other, ref multiplier);

            IDamage dmg = other.GetComponentInParent<IDamage>();

            if (dmg == null)
            {
                dmg = other.GetComponentInChildren<IDamage>();
            }

            if (dmg == null)
            {
                return;
            }

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

            dmg.takeDamage((gameManager.instance.playerWeaponManager.Damage + bonusDamage) * multiplier);
        }
    }
}
