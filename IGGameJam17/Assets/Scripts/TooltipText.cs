using TMPro;
using UnityEngine;

public class TooltipText : MonoBehaviour
{
    private TMP_Text text;
    [SerializeField] private TMP_Text costText;
    private float fadeTime = 0.2f; // Time in seconds for the tooltip to fade out

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        SetDefaultTooltip();
    }

    // Update is called once per frame
    void Update()
    {
        fadeTime -= Time.deltaTime;
        if (fadeTime < 0)
        {
            SetDefaultTooltip();
            fadeTime = 0f; // Reset fadeTime to prevent continuous resetting
        }
    }

    public void SetDefaultTooltip()
    {
        SetTooltip("Shop", "Click to purchase upgrades for points.", -1, true);
    }

    public void SetTooltip(string header, string content, int cost, bool isUnlocked)
    {
        SetText(header, content, cost, isUnlocked);
        fadeTime = 0.2f; // Reset fade time whenever a new tooltip is set
    }

    private void SetText(string header, string content, int cost, bool isUnlocked)
    {
        string costColor = (GameManager.Instance.CanAfford(cost) ? "<color=#00aa00>" : "<color=#bb0000>");
        if (isUnlocked)
            costColor = "";
        text.text = $"<b><size=140%>{header}</size></b>\n{content}";
        costText.text = cost >= 0 ? "Cost: " + costColor + cost.ToString() : string.Empty;
    }
}
