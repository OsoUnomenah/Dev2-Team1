using UnityEngine;

public class ChargeSniperScopeUI : MonoBehaviour
{
    public static ChargeSniperScopeUI Instance { get; private set; }

    [SerializeField] private GameObject scopeOverlay;

    private void Awake()
    {
        Instance = this;

        if (scopeOverlay != null)
        {
            scopeOverlay.SetActive(false);
        }
    }

    public void Show()
    {
        if (scopeOverlay != null)
        {
            scopeOverlay.SetActive(true);
        }
    }

    public void Hide()
    {
        if (scopeOverlay != null)
        {
            scopeOverlay.SetActive(false);
        }
    }
}
