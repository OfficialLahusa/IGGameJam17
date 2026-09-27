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
            text.text = (GameManager.Instance.CurrentRoundScore > 0 ? "+" : "-") + GameManager.Instance.CurrentRoundScore.ToString();
        }
        else
        {
            text.text = GameManager.Instance.CurrentRoundScore.ToString();
        }
    }
}
