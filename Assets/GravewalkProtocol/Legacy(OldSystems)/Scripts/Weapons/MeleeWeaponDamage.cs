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

        if (isAttacking)
        {
            MeleeSwing();
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
                return;
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

    // --animator based functions--
    private void OnAttackBegin()
    {
        hitEnemies.Clear();
        Debug.Log("Start damage frames");
        isAttacking = true;
    }

    private void OnAttackEnd()
    {
        isAttacking = false;
    }
    // ----------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hitPoint1.position, hitRadius);
        Gizmos.DrawSphere(hitPoint2.position, hitRadius);
    }
}


