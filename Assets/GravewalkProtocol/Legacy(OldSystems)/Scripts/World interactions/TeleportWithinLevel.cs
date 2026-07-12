using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportWithinLevel : MonoBehaviour
{
    [SerializeField] private Vector3 teleportTargetPos;
    [SerializeField] private GameObject teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (teleportTarget == null)
            {
                gameManager.instance.player.transform.position = teleportTargetPos;

            }
            else
            {
                teleportTargetPos = teleportTarget.transform.position;
                gameManager.instance.player.transform.position = teleportTargetPos;
            }
        }
    }
} 



