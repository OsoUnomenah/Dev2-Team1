using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISounds : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    [SerializeField] BaseSoundSO _UISound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (AudioManager.instance == null)
            Debug.Log("AudioManager not found");
    }

   public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.amSource.pitch = 1.3f;
            AudioManager.instance.UISound = true;
            AudioManager.instance.PlayUISound(_UISound, eventData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.amSource.pitch = 1f;
            AudioManager.instance.UISound = true;
            AudioManager.instance.PlayUISound(_UISound, eventData);
        }
    }
}
