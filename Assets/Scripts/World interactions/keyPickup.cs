using UnityEngine;

public class keyPickup : MonoBehaviour
{
    [SerializeField] keyItem key;
    private void OnTriggerEnter(Collider other)
    {
        IOpen open = other.GetComponent<IOpen>();
        if (open != null)
        {
            open.GrabKey(key);
            Destroy(gameObject);
        }
    }
}
