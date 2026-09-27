using TMPro;
using UnityEngine;

public class TooltipText : MonoBehaviour
{
    private TMP_Text text;
    private float fadeTime = 0.2f; // Time in seconds for the tooltip to fade out

    void Awake()
    {
        text = GetComponent<TMP_Text>();
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
        SetTooltip("Upgrade Shop", "Spend your scored points in the upgrade shop to enhance your odds for the coming rounds.", -1);
    }

    public void SetTooltip(string header, string content, int cost)
    {
        SetText(header, content, cost);
        fadeTime = 0.2f; // Reset fade time whenever a new tooltip is set
    }

    private void SetText(string header, string content, int cost)
    {
        string costStr = cost >= 0 ? cost.ToString() : "???";
        this.text.text = $"<size=200%><b>{header}</b></size>\n{content}\n<color=yellow>Cost: {costStr}</color>";
    }
}
