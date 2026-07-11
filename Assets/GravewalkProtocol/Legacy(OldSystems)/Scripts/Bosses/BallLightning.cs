using System.Collections;
using UnityEngine;

public class BallLightning : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] float damageRate;
    [SerializeField] float moveSpeed;

    private bool isDamaging;
    private bool isFollowing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFollowing)
        {
            StartCoroutine(FollowPlayer());
        }
    }

    IEnumerator FollowPlayer()
    {
        isFollowing = true;
        Debug.Log("Follow started");
        float timer = 0f;
        float moveTime = 1f;
        Vector3 playerPos = gameManager.instance.player.transform.position;

        while (timer < moveTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, playerPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isFollowing = false;
    }

    private void OnTriggerStay(Collider other)
    {
        IDamage dmg = gameManager.instance.playerStatHandler.GetComponentInChildren<IDamage>();

        if (other.CompareTag("Player") && !isDamaging)
        {
            Debug.Log("Player Dectected");
            StartCoroutine(DOT(dmg));
        }
    }

    IEnumerator DOT(IDamage dmg)
    {
        isDamaging = true;
        dmg.takeDamage(damage);

        yield return new WaitForSeconds(damageRate);

        isDamaging = false;
    }
}
