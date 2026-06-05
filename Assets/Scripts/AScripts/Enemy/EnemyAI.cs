using System;
using UnityEngine;
using System.Collections;

public class enemyAI : MonoBehaviour, IDamage, IInteract
{
    [SerializeField] int HP;
    [SerializeField] Renderer model;
    //[SerializeField] private TempUI tempUI; //let's it talk to the ui manager

    Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator PlaySound(BaseSoundSO sound)
    {
        if (sound != null)
        {
            GameObject soundObject = new GameObject("Temp Audio Source");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.Play();
            yield return new WaitForSeconds(0.3f);
            Destroy(soundObject);
        }
    }
    [SerializeField] private BaseSoundSO _hit;
    [SerializeField] private BaseSoundSO _dead;

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            StartCoroutine(PlaySound(_dead));
            TempUI.OffHover();
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(PlaySound(_hit));
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
    }

    IEnumerator flashGreen()
    {
        model.material.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        model.material.color = originalColor;
    }

    public void Interact()
    {
        StartCoroutine(flashGreen());
    }
    public void OnHoverEnter()
    {
        TempUI.OnHover(1);
    }
    public void OnHoverExit()
    {
        TempUI.OffHover();
    }
}
