using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class WarehouseInteract : MonoBehaviour, IOpen
{
    [SerializeField] private float doorSpeed;
    [SerializeField] private float heightFinal;
    [SerializeField] private float heightStart;

    [SerializeField] private Light hoverLight;
    [SerializeField] private Light hoverLight2;
    [SerializeField] private GameObject button;
    [SerializeField] private GameObject button2;

    [Header("Audio")]
    [SerializeField] private BaseSoundSO _raise;

    private int numInTrigger = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.transform.SetParent(null, true);
        button2.transform.SetParent(null, true);
        hoverLight.enabled = false;
        hoverLight2.enabled = false;
    }

    private IEnumerator Open()
    {
        hoverLight.enabled = true;
        hoverLight2.enabled = true;

        AudioManager.instance.PlaySoundAtPosition(_raise, gameObject);

        while (transform.position.y < heightFinal)
        {
            transform.position += Vector3.up * doorSpeed * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Close()
    {
        hoverLight.enabled = false;
        hoverLight2.enabled = false;

        yield return new WaitForSeconds(2);

        AudioManager.instance.PlaySoundAtPosition(_raise, gameObject);
        while (transform.position.y > heightStart)
        {
            transform.position -= Vector3.up * doorSpeed * Time.deltaTime;
            yield return null;
        }
    }

    public void OnHoverEnter()
    {
        Debug.Log("Raising Door");
    }

    public void OnHoverExit()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        IOpen opener = other.GetComponent<IOpen>();

        if (opener != null)
        {
            numInTrigger++;
            StartCoroutine(Open());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IOpen opener = other.GetComponent<IOpen>();

        if (opener != null)
        {
            numInTrigger--;

                if(numInTrigger <= 0)
                    StartCoroutine(Close());
        }
    }
}
