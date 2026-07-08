using UnityEngine;

public class ExplosionHandler : MonoBehaviour
{

    [SerializeField] private int damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    bool isExplode = false;
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Trigger");
           
            if (other.CompareTag("Player") && !isExplode)
            {
                gameManager.instance.playerStatHandler.takeDamage(damage);
                isExplode = true;
            }
        
    }

    
}
