using UnityEngine;

public class VignetteController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float fadeStartSeconds = 15f;

    [Header("Opacity")]
    [Range(0f, 1f)][SerializeField] private float maxAlpha = 1f;

    [Tooltip("Optional: opacity multiplier so the vignette is never fully opaque.")]
    [Range(0f, 1f)][SerializeField] private float peakOpacity = 1f;

    private SpriteRenderer spriteRenderer;
    private float currentAlpha;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || spriteRenderer == null) return;

        float targetAlpha;

        if (gm.RoundCompleted)
        {
            targetAlpha = 0f;
        }
        else
        {
            float remaining = gm.SecondsRemaining;

            if (remaining >= fadeStartSeconds)
                targetAlpha = 0f;
            else if (remaining <= 0f)
                targetAlpha = maxAlpha * peakOpacity;
            else
            {
                // 0 at fadeStartSeconds, ramps to max as remaining approaches 0
                float t = 1f - (remaining / fadeStartSeconds);
                targetAlpha = Mathf.Lerp(0f, maxAlpha * peakOpacity, t);
            }
        }

        // Smooth toward the target so it doesn't pop on reset.
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * 2f);

        Color c = spriteRenderer.color;
        c.a = currentAlpha;
        spriteRenderer.color = c;
    }
}