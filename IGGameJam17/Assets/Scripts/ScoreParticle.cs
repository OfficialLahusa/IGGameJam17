using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScoreParticle : MonoBehaviour
{
    [Header("Value")]
    [SerializeField] private int value = 1;
    [SerializeField] private bool showPlusSign = true;

    [Header("Motion")]
    [Tooltip("Base direction the particle travels (world space). Normalized at runtime.")]
    [SerializeField] private Vector2 baseDirection = new Vector2(0f, 1f);

    [Tooltip("Max random angular deviation from baseDirection, in degrees.")]
    [SerializeField] private float spreadDegrees = 25f;

    [Tooltip("Initial speed in world units per second.")]
    [SerializeField] private float initialSpeed = 4f;

    [Tooltip("How quickly speed decays. Higher = stops sooner.")]
    [SerializeField] private float speedDecay = 3f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 1f;

    [Tooltip("0..1 fraction of lifetime spent blending in.")]
    [Range(0f, 1f)][SerializeField] private float blendInFraction = 0.15f;

    [Tooltip("0..1 fraction of lifetime spent blending out at the end.")]
    [Range(0f, 1f)][SerializeField] private float blendOutFraction = 0.4f;

    [Header("Tween Curves (0..1 -> 0..1)")]
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);
    [SerializeField] private AnimationCurve velocityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Scale")]
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 1f;

    [Header("Colors")]
    [SerializeField] private Color positiveColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Tooltip("Destroy the GameObject after lifetime. If false, caller must handle it.")]
    [SerializeField] private bool destroyOnFinish = true;

    private TMP_Text _text;
    private Vector2 _velocity;
    private float _age;
    private Color _baseColor;
    private Vector3 _originalScale;

    public int Value => value;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _originalScale = transform.localScale;
    }

    public void Init(int value, Vector2? directionOverride = null)
    {
        this.value = value;

        Vector2 dir = directionOverride ?? baseDirection;
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.up;
        dir.Normalize();

        float angle = Random.Range(-spreadDegrees, spreadDegrees);
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        Vector2 rotated = new Vector2(
            dir.x * cos - dir.y * sin,
            dir.x * sin + dir.y * cos
        );

        _velocity = rotated * initialSpeed;
        _baseColor = value >= 0 ? positiveColor : negativeColor;

        string text;
        if (value > 0 && showPlusSign) text = "+" + value;
        else text = value.ToString();

        if (_text == null) _text = GetComponent<TMP_Text>();
        _text.text = text;

        _age = 0f;
        ApplyVisuals(0f);
    }

    private void Update()
    {
        _age += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(_age / lifetime) : 1f;

        float speedMul = velocityCurve.Evaluate(t);
        transform.position += (Vector3)(_velocity * speedMul * Time.deltaTime);
        _velocity = Vector2.Lerp(_velocity, Vector2.zero, speedDecay * Time.deltaTime);

        ApplyVisuals(t);

        if (t >= 1f && destroyOnFinish)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyVisuals(float t)
    {
        float alpha = alphaCurve.Evaluate(t);
        Color c = _baseColor;
        c.a *= alpha;
        _text.color = c;

        float curveScale = scaleCurve.Evaluate(t);
        float s = Mathf.Lerp(startScale, endScale, curveScale);
        transform.localScale = _originalScale * s;

        _text.enabled = alpha > 0.001f;
    }
}