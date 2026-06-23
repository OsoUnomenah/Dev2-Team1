using UnityEngine;

public class rotateObject : MonoBehaviour
{
    [SerializeField] int speed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
