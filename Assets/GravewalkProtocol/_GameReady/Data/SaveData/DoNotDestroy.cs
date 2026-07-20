using UnityEngine;

public class DoNotDestroy : MonoBehaviour
{
    private static GameObject[] persistentObjects;
    public int objectIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (persistentObjects[objectIndex] == null)
        {
            persistentObjects[objectIndex] = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (persistentObjects[objectIndex] != null)
        {
            if (persistentObjects[objectIndex] != gameObject)
            {
                Destroy(gameObject);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
