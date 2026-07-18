using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LaserEnter : MonoBehaviour
{
    [SerializeField] GameObject laser;
    private float time = 0f;
    [SerializeField] float duration;
    private bool isGrow = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Grower()
    {
        if (!isGrow)
        {            
            StartCoroutine(Grow());
        }
    }
    Vector3 start = new Vector3(0,177,0);
    Vector3 end = new Vector3(1, 177, 1);
    public IEnumerator Grow()
    {
        isGrow = true;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, time / duration);
            yield return null;
        }

        isGrow = false;
        gameObject.SetActive(false);
        transform.localScale = end;
    }
}
