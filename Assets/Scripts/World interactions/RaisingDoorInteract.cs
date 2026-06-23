using UnityEngine;
using System.Collections;

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

    [Header("Keys")]
    [SerializeField] keyItem keyExpected;

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
        if (gameManager.instance.playerInteract.keyList.Contains(keyExpected))
        {
            numInTrigger++;
            StartCoroutine(Open());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (gameManager.instance.playerInteract.keyList.Contains(keyExpected))
        {
            numInTrigger--;

                if(numInTrigger <= 0)
                    StartCoroutine(Close());
        }
    }

    public void GrabKey(keyItem key)
    {

    }
}
