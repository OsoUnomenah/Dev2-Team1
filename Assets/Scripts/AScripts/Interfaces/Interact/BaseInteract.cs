using UnityEngine;
using UnityEngine;
using System.Collections;


public class Interactor : MonoBehaviour, IInteract
{
    [SerializeField] Renderer model;

    Color originalColor;

    void Start()
    {
        originalColor = model.material.color;
    }

    public void Interact()
    {
        StartCoroutine(flashGreen());
        Debug.Log("Interact Success!");

    }

    IEnumerator flashGreen()
    {
        model.material.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        model.material.color = originalColor;
    }


}
