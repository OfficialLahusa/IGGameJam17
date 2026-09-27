using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TooltipText tooltipText;
    [SerializeField] private GameObject mainIconLocked;
    [SerializeField] private GameObject lockedSubIcon;
    [SerializeField] private GameObject unlockableSubIcon;
    [SerializeField] private string upgradeKey = "row_col_type";
    [SerializeField] private string dependencyKey = string.Empty;

    [SerializeField] private InputActionReference clickAction;

    private Camera mainCamera;
    private ShopManager shopManager;

    void Start()
    {
        mainCamera = Camera.main;

        // Init button based on whether the upgrade is unlocked and/or unlockable
        UpdateSprite();
    }

    // Update is called once per frame
    void Update()
    {
        // If mouse is hovered over the button, show the tooltip
        if (IsPointerOverThisCollider())
        {
            SetTooltip();
        }
    }

    private void OnEnable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.Enable();
            clickAction.action.started += HandlePressed;
        }

        shopManager = FindFirstObjectByType<ShopManager>();
        shopManager.OnShopUpdate += UpdateSprite;
    }

    private void OnDisable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.started -= HandlePressed;
        }

        shopManager.OnShopUpdate -= UpdateSprite;
    }

    private void UpdateSprite()
    {
        bool unlocked = IsUnlocked();
        bool unlockable = IsUnlockable();

        mainIconLocked.SetActive(!unlocked);
        lockedSubIcon.SetActive(!unlocked && !unlockable);
        unlockableSubIcon.SetActive(unlockable && !unlocked);
    }

    private void HandlePressed(InputAction.CallbackContext ctx)
    {
        if (!IsPointerOverThisCollider()) return;

        if (IsUnlockable())
        {
            Unlock();

            // Update tooltip since text is different post buy
            SetTooltip();

            // Update all sprites, including self
            shopManager.UpdateShop();

            AudioManager.Instance.PlayButtonClick();
        }
        else
        {
            AudioManager.Instance.PlayButtonFail();
        }
    }

    private bool IsUnlocked()
    {
        return GameManager.Instance.HasUpgrade(upgradeKey);
    }

    private bool IsUnlockable()
    {
        return !GameManager.Instance.HasUpgrade(upgradeKey) 
            && dependencyKey == string.Empty || GameManager.Instance.HasUpgrade(dependencyKey) 
            && GameManager.Instance.CanAfford(GetUpgradeCost(upgradeKey));
    }

    private bool Unlock()
    {
        if (!IsUnlockable())
        {
            Debug.LogError("Attempted to unlock an upgrade that is not unlockable.");
            return false;
        }

        GameManager.Instance.UnlockUpgrade(upgradeKey);
        GameManager.Instance.PayPrice(GetUpgradeCost(upgradeKey));
        return true;
    }

    private void SetTooltip()
    {
        tooltipText.SetTooltip(GetTooltipHeader(upgradeKey), GetTooltipDescription(upgradeKey), GetUpgradeCost(upgradeKey), IsUnlocked());
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        return worldPos;
    }

    private bool IsPointerOverThisCollider()
    {
        if (Mouse.current == null) return false;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return false;

        Vector3 worldPos = GetMouseWorldPos();
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        // Physics2D.OverlapPoint hits ANY collider at that point.
        // We use Collider2D.OverlapPoint on our own collider to check only ourselves.
        Collider2D collider = GetComponent<Collider2D>();
        return collider.OverlapPoint(worldPos2D);
    }

    private static string GetTooltipHeader(string upgradeKey)
    {
        return upgradeKey switch
        {
            "0_0_variant" => "Cheese",
            "1_0_variant" => "Shroom",
            "2_0_variant" => "Takoyaki",
            "0_1_time" or "0_4_time" or "1_2_time" or "2_3_time" => "Overtime",
            "0_2_variant" => "Apple",
            "0_3_stack" or "1_1_stack" or "1_4_stack" or "2_2_stack" or "2_5_stack" => "Larger Hand",
            "0_5_variant" => "Wobbly Jelly",
            "1_3_variant" => "Onigiri",
            "1_5_variant" => "Chicken",
            "2_1_variant" => "Cake",
            "2_4_variant" => "Hot Dog",
            _ => "Unknown Upgrade"
        };
    }

    private static string GetTooltipDescription(string upgradeKey)
    {
        return upgradeKey switch
        {
            "0_0_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "1_0_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "2_0_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "0_1_time" or "0_4_time" or "1_2_time" or "2_3_time" => "Increases the round duration by <color=#00aa00>+10 seconds</color>",
            "0_2_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "0_3_stack" or "1_1_stack" or "1_4_stack" or "2_2_stack" or "2_5_stack" => "You can carry <color=#00aa00>+1 item</color> at once",
            "0_5_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "1_3_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "1_5_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "2_1_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            "2_4_variant" => "New treat\n<color=#00aa00>+TODO</color> satisfaction",
            _ => "Unknown Upgrade"
        };
    }

    private static int GetUpgradeCost(string upgradeKey)
    {
        return upgradeKey switch
        {
            "0_0_variant" or "1_0_variant" or "2_0_variant" => 250,
            "0_1_time" or "1_1_stack" or "2_1_variant" => 400,
            "0_2_variant" or "1_2_time" or "2_2_stack" => 650,
            "0_3_stack" or "1_3_variant" or "2_3_time" => 1250,
            "0_4_time" or "1_4_stack" or "2_4_variant" => 1750,
            "0_5_variant" or "1_5_variant" or "2_5_stack" => 3000,
            _ => 9999
        };
    }
}
