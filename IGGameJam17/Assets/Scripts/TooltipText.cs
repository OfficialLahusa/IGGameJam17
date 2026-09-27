using TMPro;
using UnityEngine;

public class TooltipText : MonoBehaviour
{
    private string initialText;
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        initialText = text.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTooltip(string header, string content, int cost)
    {
        this.text.text = $"<size=200%><b>{header}</b></size>\n{content}\n<color=yellow>Cost: {cost}</color>";
    }
}
