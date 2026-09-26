using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        text.text = GameManager.Instance.Score.ToString();
    }
}
