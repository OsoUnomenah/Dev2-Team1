using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.WinGame();
        }
    }
}
