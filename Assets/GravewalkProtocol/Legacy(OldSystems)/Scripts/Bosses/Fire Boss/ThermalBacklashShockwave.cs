using UnityEngine;

public class ThermalBacklashShockwave : MonoBehaviour
{
    [SerializeField] private float expandSpeed = 20f;
    [SerializeField] private float maxScale = 40f;
    [SerializeField] private LayerMask coverMask;
    private int damage = 20;

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void Update()
    {
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        if (transform.localScale.x >= maxScale)
        {
            MagmaRock[] rocks = FindObjectsByType<MagmaRock>();
            foreach (MagmaRock rock in rocks)
            {
                rock.DestroyByShockwave();
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MagmaRock rock = other.GetComponentInParent<MagmaRock>();
        if (rock != null)
        {
            rock.DestroyByShockwave();
            return;
        }

        if (!other.CompareTag("Player"))
            return;

        Vector3 dir = other.transform.position - transform.position;
        float distance = dir.magnitude;

        if (Physics.Raycast(transform.position, dir.normalized, distance, coverMask))
            return;

        if (gameManager.instance != null && gameManager.instance.playerStatHandler != null)
        {
            gameManager.instance.playerStatHandler.takeDamage(damage);
        }
    }
}