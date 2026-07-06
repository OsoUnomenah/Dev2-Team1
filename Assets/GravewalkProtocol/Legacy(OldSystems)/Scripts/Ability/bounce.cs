using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class bounce : MonoBehaviour
{
    [SerializeField] private AudioClip bounceNoise;
    [SerializeField] private AudioSource audSource;

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
            audSource.PlayOneShot(bounceNoise);
        }
        Debug.LogError("Fail Bounce Pad");
    }
   
}
