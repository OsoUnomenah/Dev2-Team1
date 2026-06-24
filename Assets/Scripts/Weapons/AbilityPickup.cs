using UnityEngine;

public class AbilityPickup : MonoBehaviour, IInteract
{
    [SerializeField] private AbilityStats stats;

    [Header("Visuals")]
    [SerializeField] private Renderer model;
    [SerializeField] private Material highlight;

    private Material materialOrig;

    private void Start()
    {
        if (model != null)
        {
            materialOrig = model.material;
        }
    }

    public void Interact()
    {
        if (stats == null)
        {
            Debug.LogWarning("Ability pickup is missing AbilityStats.");
            return;
        }

        IPickupAbilities picker = gameManager.instance.playerWeaponManager.GetComponent<IPickupAbilities>();

        if (picker != null)
        {
            picker.getStats(stats);

            if (UpgradeUI.instance != null)
            {
                UpgradeUI.instance.ShowUpgradeNotification("Picked up " + GetAbilityName());
            }

            if (WeaponModHoverUI.Instance != null)
            {
                WeaponModHoverUI.Instance.HideInfo();
            }

            gameManager.instance.interactText.gameObject.SetActive(false);
            RecticleBehaviour.OffHover();

            Destroy(gameObject);
        }
    }

    public void OnHoverEnter()
    {
        if (model != null && highlight != null)
        {
            model.material = highlight;
        }

        gameManager.instance.interactText.gameObject.SetActive(true);
        RecticleBehaviour.OnHover(0);

        if (WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.ShowInfo(GetAbilityHoverText());
        }
    }

    public void OnHoverExit()
    {
        if (model != null && materialOrig != null)
        {
            model.material = materialOrig;
        }

        gameManager.instance.interactText.gameObject.SetActive(false);
        RecticleBehaviour.OffHover();

        if (WeaponModHoverUI.Instance != null)
        {
            WeaponModHoverUI.Instance.HideInfo();
        }
    }

    private string GetAbilityName()
    {
        if (stats != null && !string.IsNullOrEmpty(stats.abilityName))
        {
            return stats.abilityName;
        }

        return gameObject.name;
    }

    private string GetAbilityHoverText()
    {
        if (stats == null)
        {
            return "<b>Ability Orb</b>\nMissing ability stats";
        }

        string info = "";

        info += "<b>Ability Orb</b>\n";
        info += "<color=#66CCFF><b>" + GetAbilityName() + "</b></color>\n\n";
        info += "Level: " + stats.level + "\n";
        info += "Cooldown: " + stats.shootCooldown.ToString("0.00") + "s\n";
        info += "Effect Time: " + stats.effectTimer.ToString("0.00") + "s\n";
        info += "Shoot Distance: " + stats.shootDistance + "\n\n";
        info += "<color=#FFD966>Press E to pick up</color>";

        return info;
    }
}