using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TooltipText tooltipText;
    [SerializeField] private GameObject mainIconLocked;
    [SerializeField] private GameObject lockedSubIcon;
    [SerializeField] private GameObject unlockableSubIcon;
    [SerializeField] private int upgradeCost = 500;
    [SerializeField] private string upgradeKey = "row_col_type";
    [SerializeField] private string dependencyKey = string.Empty;
    [SerializeField] private string tooltipHeader = "Upgrade Name";
    [SerializeField] private string tooltipDescription = "Upgrade Description";

    [SerializeField] private InputActionReference clickAction;

    private Camera mainCamera;

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
    }

    private void OnDisable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.started -= HandlePressed;
        }
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

            UpdateSprite();
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
            && GameManager.Instance.CanAfford(upgradeCost);
    }

    private bool Unlock()
    {
        if (!IsUnlockable())
        {
            Debug.LogError("Attempted to unlock an upgrade that is not unlockable.");
            return false;
        }

        GameManager.Instance.UnlockUpgrade(upgradeKey);
        GameManager.Instance.PayPrice(upgradeCost);
        return true;
    }

    private void SetTooltip()
    {
        tooltipText.SetTooltip(tooltipHeader, tooltipDescription, upgradeCost);
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
}
