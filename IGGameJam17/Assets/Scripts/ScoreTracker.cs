using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField]
    private bool signPrefix = false;
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (signPrefix)
        {
            text.text = (GameManager.Instance.Score > 0 ? "+" : "-") + GameManager.Instance.Score.ToString();
        }
        else
        {
            text.text = GameManager.Instance.Score.ToString();
        }
    }
}
