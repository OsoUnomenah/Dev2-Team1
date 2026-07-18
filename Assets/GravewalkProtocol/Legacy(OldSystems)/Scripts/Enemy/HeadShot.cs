using UnityEngine;

public class HeadShot : MonoBehaviour, IDamage
{
    [SerializeField] GameObject enemy;

    private IDamage enemyDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyDamage = enemy.GetComponent<IDamage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void takeDamage(int amount)
    {
        amount *= 2;

        enemyDamage.takeDamage(amount);
    }
}
