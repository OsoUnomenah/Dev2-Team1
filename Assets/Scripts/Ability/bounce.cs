using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class bounce : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerInputHandler.
                Bounce(gameManager.instance.playerWeaponManager.abilities//[]
                [gameManager.instance.bouncePos].level);
            Debug.LogError("Bounce Pad");
        }
        Debug.LogError("Fail Bounce Pad");
    }
   
}
