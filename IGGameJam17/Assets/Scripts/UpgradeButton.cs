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
        tooltipText.SetTooltip(GetTooltipHeader(upgradeKey), GetTooltipDescription(upgradeKey), GetUpgradeCost(upgradeKey));
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
            "0_0_variant" => "Treat: Cheese",
            "0_1_time" or "0_4_time" => "Longer Rounds",
            "0_2_variant" => "Treat: Apple",
            "0_3_stack" => "Larger Hand",
            "0_5_variant" => "Treat: Wobbly Jelly",
            _ => "Unknown Upgrade"
        };
    }

    private static string GetTooltipDescription(string upgradeKey)
    {
        return upgradeKey switch
        {
            "0_0_variant" => "It's sticky and smelly, but sort of edible nonetheless...",
            "0_1_time" or "0_4_time" => "More time to score even higher, since item spawns become more frequent the longer a round lasts.",
            "0_2_variant" => "A sweet fruit that keeps scores high and doctors frightened.",
            "0_3_stack" => "Increases the maximum number of items that can be held at once.",
            "0_5_variant" => "A delightful jelly that slowly wobbles along.",
            _ => "Unknown Upgrade"
        };
    }

    private static int GetUpgradeCost(string upgradeKey)
    {
        return upgradeKey switch
        {
            "0_0_variant" => 500,
            "0_1_time" => 1000,
            "0_2_variant" => 1500,
            "0_3_stack" => 2000,
            "0_4_time" => 2500,
            "0_5_variant" => 3000,
            _ => 9999
        };
    }
}
