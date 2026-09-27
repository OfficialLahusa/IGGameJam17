using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableItem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpawnSpeed = 3f;
    public float maxSpawnSpeed = 6f;

    [Header("Rotation Settings")]
    public float minRotateSpeed = 20f;
    public float maxRotateSpeed = 90f;

    [Header("Drag Settings")]
    public float dragSmoothTime = 0.1f;

    [Header("Momentum Settings")]
    public float momentumDecay = 0f;       // Higher = momentum fades faster
    public float momentumInfluence = 1f;   // Multiplier on the release velocity
    public float maxMomentum = 20f;        // Cap so crazy flicks don't break it

    [Header("Type")]
    public bool isFood;
    public int scoreValue = 15;

    [Header("Input")]
    [SerializeField]
    private InputActionReference clickAction;

    private float rotateSpeed;
    private bool isBeingDragged = false;
    private Vector3 dragVelocity = Vector3.zero;    // Used by SmoothDamp during drag
    private Vector3 momentum = Vector3.zero;        // Carries over after release
    private Camera mainCamera;
    private static readonly float BASE_TIME_TO_LIVE = 20f; // Maximum time in seconds before the item is destroyed without interaction
    private float timeToLive = BASE_TIME_TO_LIVE; // Is refreshed upon interaction



    public bool IsFood => isFood;

    private void OnEnable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            clickAction.action.Enable();
            //clickAction.action.started += HandlePressed;
            clickAction.action.canceled += HandleReleased;
        }
    }

    private void OnDisable()
    {
        if (clickAction != null && clickAction.action != null)
        {
            //clickAction.action.started -= HandlePressed;
            clickAction.action.canceled -= HandleReleased;
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        if (Random.value > 0.5f)
            rotateSpeed = -rotateSpeed;

        RefreshTTL();
    }

    private void Update()
    {
        if (clickAction.action.IsPressed())
        {
            HandlePressed(new InputAction.CallbackContext());
        }

        if (GameManager.Instance.RoundCompleted)
        {
            isBeingDragged = false; // Prevent dragging after round ends
        }
        else
        {
            // Count object as missed if TTL expires without being dragged or sorted
            if (timeToLive <= 0f)
            {
                GameManager.Instance.AddMissedSort();

                Destroy(gameObject);
                return;
            }
        }

        if (isBeingDragged)
        {
            HandleDrag();
        }
        else
        {
            // Only subtract TTL when not dragged, so that it doesn't despawn while held.
            timeToLive -= Time.deltaTime;
            HandleFreeMovement();
        }

        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void HandleFreeMovement()
    {
        // Base rightward drift + leftover momentum
        Vector3 totalVelocity = momentum;
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

    public void SetMomentum(Vector3 newMomentum)
    {
        momentum = newMomentum;
        // Clamp momentum so extreme mouse flicks don't send it flying off screen instantly
        if (momentum.magnitude > maxMomentum)
            momentum = momentum.normalized * maxMomentum;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.RoundCompleted)
            CheckAndDestroy(collision.collider);
    }

    private void CheckAndDestroy(Collider2D other)
    {
        // Count object as missed if it touches the map border
        if (other.CompareTag("Void"))
        {
            GameManager.Instance.AddMissedSort();

            Destroy(gameObject);
            return;
        }

        // Otherwise, check if it was consumed by a target (Trash or Monster) and update score accordingly
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
            int scoreChange = assignedCorrectly ? scoreValue : -2 * scoreValue;
            GameManager.Instance.AddScore(scoreChange);
            //Debug.Log($"{gameObject.name} was consumed by {other.name} for {scoreChange}.");
            Destroy(gameObject);

            if (assignedCorrectly)
            {
                GameManager.Instance.AddCorrectSort();
            }
            else
            {
                GameManager.Instance.AddWrongSort();
            }
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
        if (!IsPointerOverThisCollider() || isBeingDragged) return;

        // Don't pick up item if stack is full
        if (GameManager.Instance.IsStackFull())
            return;

        // Otherwise, add item to stack
        GameManager.Instance.AddStackItem();
        isBeingDragged = true;
        momentum = Vector3.zero;   // Kill leftover momentum while held
        dragVelocity = Vector3.zero;

        RefreshTTL();
    }

    private void HandleReleased(InputAction.CallbackContext ctx)
    {
        if (isBeingDragged)
        {
            // Hand off the drag velocity as momentum for the free-movement phase
            momentum = dragVelocity * momentumInfluence;

            // Clamp momentum so extreme mouse flicks don't send it flying off screen instantly
            if (momentum.magnitude > maxMomentum)
                momentum = momentum.normalized * maxMomentum;

            // Randomize momentum if it's too small to avoid items getting stuck in place
            if (true || momentum.magnitude < 0.35f)
            {
                // Get direction from unit sphere sample
                Vector2 addedMomentum = Random.insideUnitCircle.normalized * Random.Range(0, 0.6f);
                momentum += new Vector3(
                    addedMomentum.x,
                    addedMomentum.y,
                    0f
                );
            }

            dragVelocity = Vector3.zero;

            isBeingDragged = false;

            RefreshTTL();
        }

        GameManager.Instance.ClearStackCount();
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void RefreshTTL()
    {
        timeToLive = BASE_TIME_TO_LIVE * (3 / minSpawnSpeed);
    }
}