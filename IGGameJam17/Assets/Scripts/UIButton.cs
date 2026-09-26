using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Button;

public class UIButton : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Input")]
    [SerializeField]
    private InputActionReference clickAction;
    [SerializeField]
    private ButtonClickedEvent clickEffect;

    private void OnEnable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.Enable();
            clickAction.action.canceled += HandleReleased;
        }
    }

    private void OnDisable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.canceled -= HandleReleased;
        }
    }

    void Awake()
    {
        mainCamera = Camera.main;
    }

    private void HandleReleased(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverThisCollider())
            clickEffect?.Invoke();
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
