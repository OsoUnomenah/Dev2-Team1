using UnityEngine;
using UnityEngine.UI;

public class TempUI : MonoBehaviour
{
    public static TempUI Instance;
    [SerializeField] public Image Reticle;
    [SerializeField] public RectTransform RetSize;

    void Awake()
    {
        Instance = this;
        GameObject r = GameObject.Find("Reticle");
        if (r != null)
        {
            Reticle = r.GetComponent<Image>();
            RetSize = r.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("Reticle not found in scene");
        }
    }


    public static void OnHover(int type)
    {
        //TempUI instance = FindFirstObjectByType<TempUI>();

        if (Instance == null || Instance.Reticle == null)
        {
            return;
        }
        switch (type)
        {
            case 0: //Interactable
                Instance.Reticle.color = new Color(0f, 0.9803922f, 0.6039216f, 1f);
                Instance.RetSize.sizeDelta = new Vector2(8, 18);
                break;
            case 1: //Enemy
                Instance.Reticle.color = new Color(1f, 0.2705882f, 0f, 1f);
                Instance.RetSize.sizeDelta = new Vector2(8, 18);
                break;
        }
    }

    public static void OffHover()
    {
       // TempUI instance = FindFirstObjectByType<TempUI>();
        if (Instance == null || Instance.Reticle == null)
        {
            return;
        }
        Instance.Reticle.color = new Color(255, 255, 255, 255);
        Instance.RetSize.sizeDelta = new Vector2(10, 10);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //eventually this will let us resize and color the retical
        Reticle.color = new Color(255, 255, 255, 255);
        RetSize.sizeDelta = new Vector2(10, 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
