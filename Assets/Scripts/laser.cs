using UnityEngine;
using System.Collections;

public class laser : MonoBehaviour, IDamage
{
    [SerializeField] LineRenderer laserLine;

    [SerializeField] GameObject hitEffect;
    [SerializeField] Transform laserStartPos;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int laserDist;

    bool isDamaging;

    // Update is called once per frame
    void Update()
    {
        createLaser();
    }

    void createLaser()
    {
        RaycastHit hit;
        if (Physics.Raycast(laserStartPos.position, laserStartPos.forward, out hit, laserDist))
        {
            
            laserLine.SetPosition(0, laserStartPos.position);
            laserLine.SetPosition(1, hit.point);
            hitEffect.SetActive(true);
            hitEffect.transform.position = hit.point;

            
            
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            IDamage playerDmg = gameManager.instance.playerStatHandler.GetComponentInChildren<IDamage>();
            if (hit.collider.CompareTag("Player"))
            {
                StartCoroutine(damageTime(playerDmg));
            }
            else if (dmg != null && !isDamaging)
            {

                // damage it
                StartCoroutine(damageTime(dmg));

            }
        } 
        else
        {
            laserLine.SetPosition(0, laserStartPos.position);
            laserLine.SetPosition(1, laserStartPos.position + (laserStartPos.forward * laserDist));
            hitEffect.SetActive(false);
        }

    }

    IEnumerator damageTime(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

    public void takeDamage(int amount)
    {
        Destroy(gameObject);
    }
}
