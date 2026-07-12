using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MeleeWeaponDamage : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Animator animator;
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
        
        if (isAttacking && !isBlocking)
        {
            MeleeSwing();
        }
        else if (isBlocking && !isAttacking)
        {
            MeleeBlock();
        }
    }

    private void MeleeSwing()
    {
 
        Collider[] hits = Physics.OverlapCapsule(hitPoint1.position, hitPoint2.position, hitRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
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
            animator.SetBool("isBlocking", false);
            isBlocking = false;
            stats.modDefense -= 100;
            defenseAdded = false;
        }
    }

    // --animator based functions--
    private void OnAttackBegin()
    {
        hitEnemies.Clear();
        // Debug.Log("Start damage frames");
        isAttacking = true;
    }

    private void OnAttackEnd()
    {
        isAttacking = false;
    }

    private void OnBlockBegin()
    {
        isBlocking = true;
    }
    // ----------------------------

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(hitPoint1.position, hitRadius);
    //    Gizmos.DrawSphere(hitPoint2.position, hitRadius);
    //}
}


