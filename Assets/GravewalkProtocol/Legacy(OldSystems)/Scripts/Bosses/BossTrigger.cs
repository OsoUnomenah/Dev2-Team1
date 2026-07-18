using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] GameObject Boss;

    private IBossTrigger bossTrig;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossTrig = Boss.GetComponent<IBossTrigger>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        bossTrig.TriggerEnter(other);
    }
}
