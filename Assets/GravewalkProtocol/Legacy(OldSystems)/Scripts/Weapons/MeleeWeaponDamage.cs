using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MeleeWeaponDamage : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform hitPoint1;
    [SerializeField] Transform hitPoint2;

    [Header("Adjustments")]
    [Range(0f, 4f)][SerializeField] float hitRadius;

    private PlayerWeaponManager weaponManager;
    private StatHandler stats;
    private LayerMask enemyLayer;

    private bool isAttacking;
    private bool isBlocking;
    private bool defenseAdded;
    private readonly HashSet<IDamage> hitEnemies = new();

    private void Start()
    {
        weaponManager = gameManager.instance.playerWeaponManager;
        stats = gameManager.instance.playerStatHandler;
        enemyLayer = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        if (gameManager.instance == null || gameManager.instance.playerInputHandler == null)
            return;
        
        if (gameManager.instance.animIsMeleeing && !gameManager.instance.animIsBlocking)
        {
            MeleeSwing();
        }
        else if (gameManager.instance.animIsBlocking && !gameManager.instance.animIsMeleeing)
        {
            MeleeBlock();
        }

        if (!gameManager.instance.animIsMeleeing && hitEnemies.Count > 0)
        {
            hitEnemies.Clear();
        }
    }

    private void MeleeSwing()
    {
        
        Collider[] hits = Physics.OverlapCapsule(hitPoint1.position, hitPoint2.position, hitRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
            //Debug.Log("Attempted damage");
            IDamage dmg = hit.GetComponentInParent<IDamage>();

           if(dmg == null)
                continue;

            if (hitEnemies.Contains(dmg))
                continue;

            hitEnemies.Add(dmg);

            if (gameManager.instance.playerInputHandler.TryShatterFrozenTarget(hit))
            {
                continue;
            }

            gameManager.instance.playerInputHandler.TryApplyWeaponFreeze(hit, true);

            int bonusDamage = 0;

            if (stats != null)
               bonusDamage = Mathf.RoundToInt(stats.modDamage);

            if (gameManager.instance.playerInputHandler.dashAttackTriggered)
            {
                // one shot normal enemies
                int x = 100 - weaponManager.Damage;
                bonusDamage += x;
            }

            dmg.takeDamage(weaponManager.Damage + bonusDamage);
        }
        gameManager.instance.playerStatHandler.HandleStamina();
    }

    private void MeleeBlock()
    {
        if (stats == null)
            return;

        if (!defenseAdded)
        {
            stats.modDefense += 100;
            defenseAdded = true;
        }

        if (!gameManager.instance.playerInputHandler.isCrouching)
        {
            //animator.SetBool("isBlocking", false);
            isBlocking = false;
            stats.modDefense -= 100;
            defenseAdded = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hitPoint1.position, hitRadius);
        Gizmos.DrawSphere(hitPoint2.position, hitRadius);
    }
}


