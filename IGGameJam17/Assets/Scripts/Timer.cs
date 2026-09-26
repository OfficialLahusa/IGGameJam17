using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        text.text = FormatSeconds((int)Mathf.Floor(GameManager.Instance.SecondsRemaining));
    }

    public static string FormatSeconds(int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes}:{seconds:00}";
    }
}
