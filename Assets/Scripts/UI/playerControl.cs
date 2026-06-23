using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class playerControl : MonoBehaviour
{

    public Slider healthbar;
    public TMP_Text healthText;
    public int currentHealth = 100;
    public int maxHealth = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHealth = 100;
        updateHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        updateHealthBar();
    }

    public void updateHealthBar()
    {
        healthText.text = currentHealth + " / " + maxHealth;
        healthbar.value = (float)currentHealth / (float)maxHealth;
    }

}
