using UnityEngine;
using UnityEngine.InputSystem;
public class FlashlightToggle : MonoBehaviour
{
    [Header("FlashLight")]
    [SerializeField] private Light flashlight;

    [Header("Settings")]
    [SerializeField] private Key toggleKey = Key.F;
    [SerializeField] private bool startsOn = false;


    private void Start()
    {
        if(flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        if(flashlight != null)
        {
            flashlight.enabled = startsOn;
        }
    }

    private void Update()
    {
        if(Keyboard.current == null)
        {
            return;
        }

        if(gameManager.instance != null)
        {
            if(gameManager.instance.isPaused || gameManager.instance.isLevelingUp)
            {
                return;
            }
        }

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        if(flashlight == null)
        {
            return;
        }

        flashlight.enabled = !flashlight.enabled;
    }

        
}
