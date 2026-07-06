using TMPro;
using UnityEngine;

public class WeaponModHoverUI : MonoBehaviour
{
    public static WeaponModHoverUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text infoText;

    private void Awake()
    {
        Instance = this;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void ShowInfo(string message)
    {
        if (panel == null || infoText == null)
        {
            return;
        }

        infoText.text = message;
        panel.SetActive(true);
    }

    public void HideInfo()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}
