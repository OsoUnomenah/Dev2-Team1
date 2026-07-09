using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FloorBombs : MonoBehaviour, IDamage
{
    [Header("Bomb Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private float explotionTimer;
    [SerializeField] private float riseTimer;

    private Renderer rend;
    [SerializeField] Material original;
    [SerializeField] Material redFlash;
    [SerializeField] private GameObject explotion;

    [SerializeField] private BaseSoundSO _explode;

    Vector3 startPos;
    Vector3 endPos;

    private bool isRisen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponent<Renderer>();

        startPos = transform.position;
        endPos = startPos + Vector3.up *5f;

        StartCoroutine(Rise());

    }

    bool isExploding = false;
    // Update is called once per frame
    void Update()
    {
        if(isRisen)
        {
            isExploding = true;
            StartCoroutine(Explode());            
        }
    }
    
    
    IEnumerator End()
    {
        AudioManager.instance.PlaySound(_explode);
        explotion.SetActive(true);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    IEnumerator Rise()
    {
        float timer = 0f;
        while (timer < riseTimer)
        {
            timer += Time.deltaTime;

            float percent = timer / riseTimer;
            percent = Mathf.SmoothStep(0f, 1f, percent);

            transform.position = Vector3.Lerp(startPos, endPos, percent);

            yield return null;
        }
        transform.position = endPos;
        isRisen = true;
    }

    IEnumerator Explode()
    {
        float timer = 0f;
        float flashInterval = 0.5f;
        float flashSpeed = 1f;
        float fastFlash = explotionTimer - 3;

        while (timer < explotionTimer)
        {
            timer += Time.deltaTime;

            float flash = Mathf.PingPong( timer * flashSpeed, 1f);

            if(timer > fastFlash)
            {
               flashSpeed = 2f;
            }

            if (flash < flashInterval)
            {
                rend.material.color = Color.Lerp(original.color, redFlash.color, flash * 2);
            }
            else
            {
                rend.material.color = Color.Lerp(redFlash.color, original.color, (flash - 0.5f) * 2);
            }

            yield return null;
        }

        StartCoroutine(End());
    }

    public void takeDamage(int amount)
    {
        currentHealth -= amount;
        if(currentHealth < 0)
        {
            Destroy(gameObject);
        }
    }
}
