using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class WarehouseInteract : MonoBehaviour, IOpen
{
    [Header("Behavior")]
    [SerializeField] private float doorSpeed;
    [SerializeField] private float heightFinal;
    [SerializeField] private float heightStart;

    [Header("Children")]
    [SerializeField] private Light hoverLight;
    [SerializeField] private Light hoverLight2;
    [SerializeField] private GameObject button;
    [SerializeField] private GameObject button2;
    [SerializeField] private GameObject lockedText;

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
        IOpen opener = other.GetComponent<IOpen>();

        if (gameManager.instance.playerInteract.keyList.Contains(keyExpected) || (keyExpected == null && opener != null))
        {
            numInTrigger++;
            StartCoroutine(Open());
        }
        else if(keyExpected != null && !gameManager.instance.playerInteract.keyList.Contains(keyExpected))
        {
            lockedText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IOpen opener = other.GetComponent<IOpen>();

        if (gameManager.instance.playerInteract.keyList.Contains(keyExpected) || (keyExpected == null && opener != null))
        {
            numInTrigger--;

                if(numInTrigger <= 0)
                    StartCoroutine(Close());
        }
        else if (opener != null)
        {
            lockedText.SetActive(false);
        }

    }

    public void GrabKey(keyItem key)
    {

    }
}
