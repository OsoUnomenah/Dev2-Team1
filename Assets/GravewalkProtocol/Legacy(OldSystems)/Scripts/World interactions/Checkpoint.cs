using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameManager.instance.playerSpawnPos.transform.position != transform.position)
        {
            gameManager.instance.playerSpawnPos.transform.position = transform.position;
            StartCoroutine(CheckpointFlash());

            Debug.Log("Checkpoint reached!");
        }
    }

    IEnumerator CheckpointFlash()
    {
        gameManager.instance.checkpointUI.SetActive(true);
        yield return new WaitForSeconds(2f);
        gameManager.instance.checkpointUI.SetActive(false);
    }
}
