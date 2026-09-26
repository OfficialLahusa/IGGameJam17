using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float SecondsRemaining { get; private set; } = 120f;
    public int Score { get; private set; }
    public bool RoundCompleted { get; private set; } = false;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (!RoundCompleted)
        {
            SecondsRemaining -= Time.deltaTime;

            if (SecondsRemaining <= 0f)
            {
                RoundCompleted = true;
                SecondsRemaining = 0f;

                Debug.Log("Round completed!");
            }
        }
    }

    public void AddScore(int score)
    {

    }
}
