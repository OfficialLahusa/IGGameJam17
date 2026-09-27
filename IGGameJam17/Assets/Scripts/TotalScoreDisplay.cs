using TMPro;
using UnityEngine;

public class TotalScoreDisplay : MonoBehaviour
{
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        text.text = $"Total Score: {GameManager.Instance.TotalScore}";
    }
}
