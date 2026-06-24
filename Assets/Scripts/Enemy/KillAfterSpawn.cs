using UnityEngine;
using System.Collections;

public class KillAfterSpawn : MonoBehaviour
{

    private void Start()
    { 
        StartCoroutine(DestroyAferSec()); 
    }

    IEnumerator DestroyAferSec()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
