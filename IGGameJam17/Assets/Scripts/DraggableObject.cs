using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableItem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minMoveSpeed = 1f;
    [SerializeField] private float maxMoveSpeed = 3f;

    [Header("Rotation Settings")]
    [SerializeField] private float minRotateSpeed = 20f;
    [SerializeField] private float maxRotateSpeed = 90f;

    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothTime = 0.15f;

    [Header("Momentum Settings")]
    [SerializeField] private float momentumDecay = 2f;       // Higher = momentum fades faster
    [SerializeField] private float momentumInfluence = 1f;   // Multiplier on the release velocity
    [SerializeField] private float maxMomentum = 20f;        // Cap so crazy flicks don't break it

    [Header("Type")]
    [SerializeField] private bool isFood;
    [SerializeField] private int scoreValue = 10;

    [Header("Input")]
    [SerializeField]
    private InputActionReference clickAction;

    private float moveSpeed;
    private float rotateSpeed;
    private bool isBeingDragged = false;
    private Vector3 dragVelocity = Vector3.zero;    // Used by SmoothDamp during drag
    private Vector3 momentum = Vector3.zero;        // Carries over after release
    private Camera mainCamera;

    

    public bool IsFood => isFood;

    private void OnEnable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.Enable();
            clickAction.action.started += HandlePressed;
            clickAction.action.canceled += HandleReleased;
        }
    }

    private void OnDisable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.started -= HandlePressed;
            clickAction.action.canceled -= HandleReleased;
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        if (Random.value > 0.5f)
            rotateSpeed = -rotateSpeed;
    }

    private void Update()
    {
        if (isBeingDragged)
        {
            HandleDrag();
        }
        else
        {
            HandleFreeMovement();
        }

        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void HandleFreeMovement()
    {
        // Base rightward drift + leftover momentum
        Vector3 totalVelocity = (Vector3.right * moveSpeed) + momentum;
        transform.position += totalVelocity * Time.deltaTime;

        // Decay momentum toward zero
        momentum = Vector3.Lerp(momentum, Vector3.zero, momentumDecay * Time.deltaTime);

        // Snap tiny values to zero to avoid perpetual micro-drift
        if (momentum.sqrMagnitude < 0.0001f)
            momentum = Vector3.zero;
    }

    private void HandleDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        mouseWorldPos.z = 0f;

        Vector3 previousPos = transform.position;

        // Follow the mouse smoothly — SmoothDamp's velocity output is our "drag velocity"
        transform.position = Vector3.SmoothDamp(
            transform.position,
            mouseWorldPos,
            ref dragVelocity,
            dragSmoothTime
        );

        // Store actual measured velocity for release (more reliable than SmoothDamp velocity alone)
        if (Time.deltaTime > 0f)
        {
            dragVelocity = (transform.position - previousPos) / Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndDestroy(collision.collider);
    }

    private void CheckAndDestroy(Collider2D other)
    {
        bool consumed = false;
        bool assignedCorrectly = false;

        // Hit Trash
        if (other.CompareTag("TrashCan"))
        {
            consumed = true;
            assignedCorrectly = !isFood;
        }
        // Hit Monster
        else if (other.CompareTag("Monster"))
        {
            consumed = true;
            assignedCorrectly = isFood;
        }

        if (consumed)
        {
            int scoreChange = assignedCorrectly ? scoreValue : -scoreValue;
            GameManager.Instance.AddScore(scoreChange);
            Debug.Log($"{gameObject.name} was consumed by {other.name} for {scoreChange}.");
            GameManager.Instance.SpawnItem();
            Destroy(gameObject);
        }
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

    private void HandlePressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Down");


        if (!IsPointerOverThisCollider()) return;

        isBeingDragged = true;
        momentum = Vector3.zero;   // Kill leftover momentum while held
        dragVelocity = Vector3.zero;

    }

    private void HandleReleased(InputAction.CallbackContext ctx)
    {
        Debug.Log("Down");


        isBeingDragged = false;

        // Hand off the drag velocity as momentum for the free-movement phase
        momentum = dragVelocity * momentumInfluence;

        // Clamp momentum so extreme mouse flicks don't send it flying off screen instantly
        if (momentum.magnitude > maxMomentum)
            momentum = momentum.normalized * maxMomentum;

        dragVelocity = Vector3.zero;
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}