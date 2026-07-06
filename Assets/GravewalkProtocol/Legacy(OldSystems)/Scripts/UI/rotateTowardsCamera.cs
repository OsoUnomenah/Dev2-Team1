using UnityEngine;

public class rotateTowardsCamera : MonoBehaviour
{
    public Camera playerCamera;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = gameManager.instance.playerCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCamera != null)
        {
            transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward, playerCamera.transform.rotation * Vector3.up);
        }
    }
}
