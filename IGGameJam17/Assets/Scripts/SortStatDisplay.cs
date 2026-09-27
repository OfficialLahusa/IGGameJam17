using TMPro;
using UnityEngine;

public class SortStatDisplay : MonoBehaviour
{
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        int total = GameManager.Instance.RoundCorrectSorts + GameManager.Instance.RoundWrongSorts + GameManager.Instance.RoundMissedSorts;
        float correctPercent = total > 0 ? (float)GameManager.Instance.RoundCorrectSorts / total * 100f : 0f;
        float wrongPercent = total > 0 ? (float)GameManager.Instance.RoundWrongSorts / total * 100f : 0f;
        float missedPercent = total > 0 ? (float)GameManager.Instance.RoundMissedSorts / total * 100f : 0f;

        text.text = $"<color=green>{correctPercent:F1}% correct</color>\r\n<color=red>{wrongPercent:F1}% wrong</color>\r\n<color=orange>{missedPercent:F1}% missed</color>";
    }
}
